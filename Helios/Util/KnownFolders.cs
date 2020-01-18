using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GadrocsWorkshop.Helios.Util
{
    public class KnownFolders
    {
        private static Guid FolderAppDataRoaming = new Guid("3EB685DB-65F9-4CF6-A03A-E3EF65729F3D");

        public static string AppDataRoaming
        {
            get
            {
                IntPtr pathPtr;
                string appDataRoaming;
                int hr = NativeMethods.SHGetKnownFolderPath(ref FolderAppDataRoaming, 0, IntPtr.Zero, out pathPtr);
                if (hr == 0)
                {
                    appDataRoaming = System.Runtime.InteropServices.Marshal.PtrToStringUni(pathPtr);
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(pathPtr);
                }
                else
                {
                    appDataRoaming = Environment.GetEnvironmentVariable("userprofile") + "AppData\\Roaming";
                }
                return appDataRoaming;
            }
        }


        private static Guid FolderSavedGames = new Guid("4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4");

        public static string SavedGames
        {
            get
            {
                // We attempt to get the Saved Games known folder from the native method to cater for situations
                // when the locale of the installation has the folder name in non-English.
                IntPtr pathPtr;
                string savedGamesPath;
                int hr = NativeMethods.SHGetKnownFolderPath(ref FolderSavedGames, 0, IntPtr.Zero, out pathPtr);
                if (hr == 0)
                {
                    savedGamesPath = System.Runtime.InteropServices.Marshal.PtrToStringUni(pathPtr);
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(pathPtr);
                }
                else
                {
                    savedGamesPath = Environment.GetEnvironmentVariable("userprofile") + "Saved Games";
                }
                return savedGamesPath;
            }
        }
    }
}
