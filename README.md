# AstroJet

Android port workspace for the supplied Jets'n'Guns 2 Windows installation.
The first test device is a Samsung Galaxy S21 5G running Android 15. The Android
application targets ARM64 and landscape orientation.

## Current status

- Recovered the game, physics and Spine runtime sources. The unmodified game
  passes a compiler check against its original dependencies.
- Decoded all 398 text and level files, containing 345,535 nonempty lines.
  An encryption round-trip verified one sample from every decoded file.
- Repacked the content archive from 996,041,820 bytes to 288,204,526 bytes.
  All 2,675 file names, sizes and CRCs match the original archive; decompression
  and CRC verification also passed for every repacked entry.
- Added multitouch movement, weapon buttons, pause input and menu tapping,
  plus an on-screen control overlay.
- Added an Android SoundPool and MediaPlayer backend for effects and music.
- Converted all three graphics-effect containers from MGFX 8 to MGFX 10,
  preserving the embedded GLSL and checking the parsed shader models.
- Added an Android application project with first-launch asset installation.
- The current Android sources pass a compiler check against Android reference
  assemblies. This is not a completed Android package build or a device test.

**No APK is available yet.** The Android workload installation failed while
downloading required packages. Java and Android SDK setup, a complete Android
build, and on-device verification are still pending. Audio, shader rendering,
touch layout, performance, pause/resume and save persistence need device tests.

## Layout

| Directory | Purpose |
| --- | --- |
| `recovered/` | Unmodified recovery baseline, including original assembly metadata |
| `src/Game/` | Working game sources adapted for current MonoGame |
| `src/Mobile/` | Touch controls, overlay and Android audio |
| `src/Android/` | Android application and first-launch asset installation |
| `lib/` | Managed dependencies from the supplied installation |
| `tools/` | Source recovery, data export, packaging, conversion and build scripts |
| `verification/` | Touch-control checks; generated compiler logs stay local |
| `packed/` | Local prepared game content, excluded from Git |

Large game assets, downloaded development tools, native Windows binaries and
generated Android packages are excluded from Git. Original copyright metadata
is retained in recovered and adapted sources.

## Prepare the content

Use Python 3 and the supplied game installation. These commands read the
installation and write a separate `packed/` directory inside this repository:

```powershell
python tools/package_content.py --game-dir 'C:\path\to\Jets n Guns 2'
python tools/convert_effects.py --game-dir 'C:\path\to\Jets n Guns 2'
```

Run shader conversion after packaging, because packaging copies the original
shader files. The converter checks container structure; GPU rendering still
needs a device test.

## Build the Android application

Install the .NET 10 SDK and its Android workload, with the matching Android SDK
and Java dependencies. The local development SDK, when present, is under
`tools/dotnet/`. Install the workload with:

```powershell
dotnet workload install android
```

After preparing the content and the Android toolchain:

```powershell
./tools/Build-Android.ps1 -AndroidSdkDirectory 'C:\path\to\android-sdk' -JavaSdkDirectory 'C:\path\to\jdk'
```

The application ID is `local.yaser.jng2port`. This is an early local test build.
The first launch copies bundled assets into private app storage before starting
the recovered game runtime. Save files use a separate private application
directory.

## Verification

PowerShell 7 can run the touch-controller checks:

```powershell
./tools/Verify-TouchControls.ps1
```

`Verify-RecoveredSource.ps1` performs compiler diagnostics using Roslyn from the
PowerShell runtime. Its optional Android compatibility mode uses locally
downloaded MonoGame and Android reference assemblies. It does not replace a
full Android build, generated Android bindings, resource packaging or device
testing. Downloaded reference assemblies are intentionally excluded from Git.

As the port grows, a knowledge graph can help trace level loading, rendering,
physics and input relationships. No graph has been created yet.
