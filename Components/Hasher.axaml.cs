using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Components;

public static class DirectoryHasher
{
    public static string CreateDirectorySha256(string srcPath)
    {
        var filePaths = Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories)
            .OrderBy(p => p)
            .ToArray();

        using (var sha256 = SHA256.Create())
        {
            foreach (var filePath in filePaths)
            {
                string relativePath = Path.GetRelativePath(srcPath, filePath).Replace('\\', '/');

                // Hash relative path
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath);
                sha256.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // Hash file contents
                byte[] contentBytes = File.ReadAllBytes(filePath);
                sha256.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }

            // Finalize the hash
            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            return BitConverter.ToString(sha256.Hash ?? Array.Empty<byte>()).Replace("-", "").ToLowerInvariant();
        }
    }
}