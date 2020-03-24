using System.Collections.Generic;
using System.IO;

namespace GoogleDiff
{
    class GoogleDiff
    {
        /// <summary>
        /// performs a google diff between stdin and the file identified by args[0] and outputs to stdout
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                throw new System.Exception("modified file path argument is required");
            }
            DiffMatchPatch.diff_match_patch googleDiff = new DiffMatchPatch.diff_match_patch();

            // read stdin raw, including separators
            string source = ReadStdin();
            string target;
            using (StreamReader streamReader = new StreamReader(args[0], System.Text.Encoding.UTF8))
            {
                target = streamReader.ReadToEnd();
            }

            // NOTE: do our own diffs so we just do semantic cleanup. We don't want to optimize for efficiency.
            List<DiffMatchPatch.Diff> diffs;
            if ((args.Length > 1) && (args[1] == "--reverse"))
            {
                diffs = googleDiff.diff_main(target, source);
            }
            else
            {
                diffs = googleDiff.diff_main(source, target);
            }

            googleDiff.diff_cleanupSemantic(diffs);
            List<DiffMatchPatch.Patch> patches = googleDiff.patch_make(diffs);
            System.Console.Write(googleDiff.patch_toText(patches));
        }

        private static string ReadStdin()
        {
            List<string> strings = new List<string>();
            Stream inputStream = System.Console.OpenStandardInput();
            using (inputStream) {
                byte[] buffer = new byte[2 * 1024 * 1024];
                for (; ; )
                {
                    int read = inputStream.Read(buffer, 0, buffer.Length);
                    if (read < 1)
                    {
                        break;
                    }
                    strings.Add(System.Text.Encoding.UTF8.GetString(buffer, 0, read));
                }
                return string.Join("", strings);
            }
        }
    }
}
