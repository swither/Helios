using System;
using System.Collections;
using System.Collections.Specialized;

namespace GadrocsWorkshop.Helios.Util
{
    public class Anonymizer
    {
        private static readonly OrderedDictionary Replacements = new OrderedDictionary
        {
            // replace home directory reference
            {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "%USERPROFILE%"},
            // and file:// URIS
            {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace("\\", "/"), "%USERPROFILE%"},
            // just in case user name appears in other paths
            {$"\\{Environment.UserName}\\", "\\%USERNAME%\\"},
            // and URIs
            {$"/{Environment.UserName}/", "/%USERNAME%/"}
        };

        public static string Anonymize(string value)
        {
            string working = value;
            IDictionaryEnumerator iterator = Replacements.GetEnumerator();
            while (iterator.MoveNext())
            {
                working = working.Replace((string)iterator.Key, (string)iterator.Value);
            }

            return working;
        }

        public static string Anonymize(Uri uri)
        {
            // nothing special so far
            return Anonymize((uri.ToString()));
        }
    }
}