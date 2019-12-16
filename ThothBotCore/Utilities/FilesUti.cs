using System.IO;

namespace ThothBotCore.Utilities
{
    public class FilesUti
    {
        string GetFilesInDirectory(string path)
        {
            Directory.EnumerateFiles(path);
            return "finish this bro";
        }
    }
}
