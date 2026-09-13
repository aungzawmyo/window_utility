using System.IO;
using System.Text;

namespace DevWindowUtility.Services;

public sealed class TextFileService
{
    public const long MaxBytes = 8 * 1024 * 1024;

    public string Read(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
        {
            throw new FileNotFoundException("File not found.", path);
        }

        if (info.Length > MaxBytes)
        {
            throw new InvalidOperationException($"File is larger than {MaxBytes / (1024 * 1024)} MB.");
        }

        return File.ReadAllText(path, Encoding.UTF8);
    }

    public void Write(string path, string text)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
