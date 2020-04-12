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
            // just in case user name appears in other paths
            {$"\\{Environment.UserName}\\", "\\%USERNAME%\\"}
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
    }
}