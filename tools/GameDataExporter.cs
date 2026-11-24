using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AndroidPort.Tools
{
    public static class GameDataExporter
    {
        public static string Export(string archivePath, string destination, string password)
        {
            // Match the recovered game's UTF-16, PBKDF2-SHA1 and AES-CBC content reader.
            byte[] salt = Encoding.ASCII.GetBytes("Ivan Medvedev");
            byte[] material = Rfc2898DeriveBytes.Pbkdf2(password, salt, 1000, HashAlgorithmName.SHA1, 48);
            using Aes aes = Aes.Create();
            aes.Key = material[..32];
            aes.IV = material[32..48];
            int files = 0, encodedFiles = 0, decodedLines = 0, verifiedLines = 0;
            string outputRoot = Path.GetFullPath(destination) + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(outputRoot);
            using var archive = ZipFile.OpenRead(archivePath);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string extension = Path.GetExtension(entry.FullName);
                if (extension != ".txt" && extension != ".jnl") continue;
                string output = Path.GetFullPath(Path.Combine(outputRoot, entry.FullName));
                if (!output.StartsWith(outputRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Archive entry escapes the export directory.");
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8, true);
                string firstLine = reader.ReadLine();
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                using var writer = new StreamWriter(output, false, new UTF8Encoding(false));
                bool encoded = firstLine == "JnGeNc";
                if (!encoded) writer.WriteLine(firstLine);
                else encodedFiles++;
                string line;
                bool verifiedFile = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (encoded)
                    {
                        byte[] input = Convert.FromBase64String(line.Replace(" ", "+"));
                        if (input.Length == 0) { writer.WriteLine(); continue; }
                        using var decryptor = aes.CreateDecryptor();
                        byte[] plain = decryptor.TransformFinalBlock(input, 0, input.Length);
                        if (plain.Length % 2 != 0) throw new InvalidDataException("Invalid UTF-16 data: " + entry.FullName);
                        if (!verifiedFile)
                        {
                            using var encryptor = aes.CreateEncryptor();
                            byte[] roundTrip = encryptor.TransformFinalBlock(plain, 0, plain.Length);
                            if (!input.AsSpan().SequenceEqual(roundTrip))
                                throw new InvalidDataException("Content round-trip mismatch: " + entry.FullName);
                            verifiedFile = true;
                            verifiedLines++;
                        }
                        writer.WriteLine(Encoding.Unicode.GetString(plain));
                        decodedLines++;
                    }
                    else writer.WriteLine(line);
                }
                files++;
            }
            return $"Exported {files} text/level files; decoded {encodedFiles} encoded files and {decodedLines} nonempty lines. Verified {verifiedLines} file samples by encryption round-trip.";
        }
    }
}
