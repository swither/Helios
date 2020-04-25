using System.Text.RegularExpressions;

namespace GadrocsWorkshop.Helios.Util
{
    public static class KnownLinks
    {
        /// <summary>
        /// the root URL of our origin git repo, or null
        /// </summary>
        /// <returns>the URL if we can figure it out, or null</returns>
        public static string GitRepoUrl()
        {
            string repo = ConfigManager.SettingsManager.LoadSetting("Helios", "Repo", null);

            if (repo == null)
            {
                repo = ConfigManager.SettingsManager.LoadSetting("Helios", "LastestGitHubDownloadUrl", null);
            }
            else
            {
                if (!repo.EndsWith("/"))
                {
                    repo += ('/');
                }
            }

            if (repo == null)
            {
                // last resort
                repo = "https://github.com/BlueFinBima/Helios/";
            }

            Match match = new Regex("^https://github.com/[a-zA-Z0-9_]+/[a-zA-Z0-9_]+/").Match(repo);
            return match.Success ? match.Groups[0].Value : null;
        }
    }
}