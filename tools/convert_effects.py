"""Convert legacy MGFX 8 containers to MGFX 10, preserving embedded GLSL.

The layouts are taken from the original and Android Effect/Shader readers saved
under research/. GPU compilation and visual equivalence still need device tests.
"""
from pathlib import Path
import struct
import json
import argparse


class Reader:
    def __init__(self, data, version):
        self.data, self.version, self.pos = data, version, 10

    def take(self, count):
        result = self.data[self.pos:self.pos + count]
        if len(result) != count:
            raise ValueError("Unexpected end of effect")
        self.pos += count
        return result

    def byte(self):
        return self.take(1)[0]

    def count(self):
        return self.byte() if self.version == 8 else struct.unpack('<i', self.take(4))[0]

    def string(self):
        start, length, shift = self.pos, 0, 0
        while True:
            item = self.byte()
            length |= (item & 127) << shift
            if not item & 128:
                break
            shift += 7
            if shift > 28:
                raise ValueError("Invalid string length")
        self.take(length)
        return self.data[start:self.pos]

    def annotations(self):
        count = self.count()
        if count:
            raise ValueError("Nonempty effect annotations need a dedicated converter")
        return count

    def parameters(self):
        parameters = []
        for _ in range(self.count()):
            kind, value_type = self.byte(), self.byte()
            name, semantic = self.string(), self.string()
            annotations = self.annotations()
            rows, columns = self.byte(), self.byte()
            elements, members = self.parameters(), self.parameters()
            values = self.take(rows * columns * 4) if not elements and not members and value_type in (1, 2, 3) else b''
            if value_type == 4:
                raise ValueError("String shader parameters are unsupported")
            parameters.append((kind, value_type, name, semantic, annotations, rows, columns, elements, members, values))
        return parameters

    def shader(self):
        start = self.pos
        self.take(1)  # stage
        size = struct.unpack('<i', self.take(4))[0]
        self.take(size)  # GLSL is copied byte for byte
        for _ in range(self.byte()):
            self.take(3)  # sampler type and slots
            if self.byte():
                self.take(20)  # sampler state
            self.string()
            self.take(1)  # parameter index remains a byte in MGFX 10
        self.take(self.byte())  # constant buffer indices
        for _ in range(self.byte()):
            self.string()
            self.take(4)  # usage, index and signed location
        return self.data[start:self.pos]


def parse(data):
    if data[:4] != b'MGFX' or data[4] not in (8, 10) or data[5] != 0:
        raise ValueError('Expected OpenGL MGFX 8 or 10')
    reader = Reader(data, data[4])
    buffers = []
    for _ in range(reader.count()):
        name, size = reader.string(), reader.take(2)
        entries = [(reader.count(), reader.take(2)) for _ in range(reader.count())]
        buffers.append((name, size, entries))
    shaders = [reader.shader() for _ in range(reader.count())]
    parameters = reader.parameters()
    techniques = []
    for _ in range(reader.count()):
        name, annotations = reader.string(), reader.annotations()
        passes = []
        for _ in range(reader.count()):
            pass_name, pass_annotations = reader.string(), reader.annotations()
            vertex, pixel = reader.count(), reader.count()
            if data[4] == 8:
                vertex = -1 if vertex == 255 else vertex
                pixel = -1 if pixel == 255 else pixel
            states = []
            for length in (18, 25, 12):
                states.append(reader.take(length) if reader.byte() else None)
            passes.append((pass_name, pass_annotations, vertex, pixel, states))
        techniques.append((name, annotations, passes))
    if data[4] == 10 and reader.take(4) != b'MGFX':
        raise ValueError('Missing MGFX footer')
    if reader.pos != len(data):
        raise ValueError(f'Unconsumed effect bytes: {len(data) - reader.pos}')
    return buffers, shaders, parameters, techniques


def convert(data):
    model = parse(data)
    output = bytearray(b'MGFX\x0a\x00' + data[6:10])
    def count(value):
        output.extend(struct.pack('<i', value))
    def parameters(items):
        count(len(items))
        for kind, value_type, name, semantic, annotations, rows, columns, elements, members, values in items:
            output.extend(bytes((kind, value_type)) + name + semantic)
            count(annotations)
            output.extend(bytes((rows, columns)))
            parameters(elements)
            parameters(members)
            output.extend(values)
    buffers, shaders, params, techniques = model
    count(len(buffers))
    for name, size, entries in buffers:
        output.extend(name + size)
        count(len(entries))
        for index, offset in entries:
            count(index)
            output.extend(offset)
    count(len(shaders))
    for shader in shaders:
        output.extend(shader)
    parameters(params)
    count(len(techniques))
    for name, annotations, passes in techniques:
        output.extend(name)
        count(annotations)
        count(len(passes))
        for name, annotations, vertex, pixel, states in passes:
            output.extend(name)
            count(annotations)
            count(vertex)
            count(pixel)
            for state in states:
                output.append(state is not None)
                if state is not None:
                    output.extend(state)
    output.extend(b'MGFX')
    if parse(bytes(output)) != model:
        raise ValueError('Converted shader model differs from original')
    return bytes(output), len(shaders), len(params)


if __name__ == '__main__':
    workspace = Path(__file__).resolve().parents[1]
    arguments = argparse.ArgumentParser(description=__doc__)
    arguments.add_argument('--game-dir', type=Path, default=workspace.parent)
    root = arguments.parse_args().game_dir.resolve()
    destination = workspace / 'packed/shader'
    destination.mkdir(parents=True, exist_ok=True)
    report = []
    for source in (root / 'shader').glob('*.mgfxo'):
        converted, shaders, parameters = convert(source.read_bytes())
        (destination / source.name).write_bytes(converted)
        report.append({'file': source.name, 'version': 10, 'shader_stages': shaders,
                       'parameters': parameters, 'model_verified': True, 'device_tested': False})
    (destination.parent / 'shader-conversion-result.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps(report, indent=2))
