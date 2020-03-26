using System.IO;

namespace ToolsCommon
{
    public class FileSystem
    {
        public static bool TryFindNearestDirectory(string path, out string absolutePath)
        {
            string cwd = Directory.GetCurrentDirectory();
            DirectoryInfo info = new DirectoryInfo(cwd);
            while (info != null)
            {
                string candidate = Path.Combine(info.FullName, path);
                if (Directory.Exists(candidate))
                {
                    candidate += Path.DirectorySeparatorChar;
                    absolutePath = candidate;
                    return true;
                }
                info = info.Parent;
            }
            absolutePath = null;
            return false;
        }

        public static string FindNearestDirectory(string path)
        {
            if (!TryFindNearestDirectory(path, out string result))
            {
                throw new System.IO.DirectoryNotFoundException($"could not find ancestor directory that contains '{path}'");
            }
            return result;
        }
    }
}
