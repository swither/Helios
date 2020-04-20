using LibGit2Sharp;
using System;

namespace InitSubmodules
{
    class InitSubmodules
    {
        static void Main(string[] args)
        {
            string repoRoot = Repository.Discover(Environment.CurrentDirectory) ??
                throw new System.Exception("InitSubmodules must be run within a git repo.");
            using (Repository repo = new Repository(repoRoot))
            {
                foreach (Submodule submodule in repo.Submodules)
                {
                    SubmoduleStatus submoduleStatus = submodule.RetrieveStatus();
                    bool needsInit = submoduleStatus.HasFlag(SubmoduleStatus.WorkDirUninitialized);
                    if (needsInit)
                    {
                        repo.Submodules.Update(submodule.Name, new SubmoduleUpdateOptions
                        {
                            Init = true
                        });
                    }
                }
            }
        }
    }
}
