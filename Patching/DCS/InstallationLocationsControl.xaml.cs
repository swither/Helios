using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// Interaction logic for InstallationLocationsControl.xaml
    /// </summary>
    public partial class InstallationLocationsControl : ItemsControl
    {
        static InstallationLocationsControl()
        {
            System.Type ownerType = typeof(InstallationLocationsControl);
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(AddInstallationLocationCommand, Add_Executed));
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(RemoveInstallationLocationCommand, Remove_Executed));
        }

        public InstallationLocationsControl()
        {
            DataContext = InstallationLocations.Singleton;
            InitializeComponent();
        }

        #region Commands
        public static readonly RoutedUICommand AddInstallationLocationCommand = new RoutedUICommand("Adds a new DCS installation location.", "AddInstallationLocation", typeof(InstallationLocationsControl));
        public static readonly RoutedUICommand RemoveInstallationLocationCommand = new RoutedUICommand("Removes a DCS installation location.", "RemoveInstallationLocation", typeof(InstallationLocationsControl));

        private static void Add_Executed(object target, ExecutedRoutedEventArgs e)
        {
            List<string> guesses = GenerateDcsRootDirectoryGuesses();

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            string guessName = InstallationLocation.AUTO_UPDATE_CONFIG;
            openFileDialog.Title = "Navigate to DCS Installation";
            openFileDialog.InitialDirectory = "";
            openFileDialog.FileName = guessName;
            openFileDialog.DefaultExt = ".cfg";
            openFileDialog.Filter = $"DCS|{InstallationLocation.AUTO_UPDATE_CONFIG}";
            openFileDialog.CheckFileExists = true;

            foreach (string guess in guesses)
            {
                if (openFileDialog.InitialDirectory == "")
                {
                    openFileDialog.InitialDirectory = guesses[0];
                }
                else
                {
                    openFileDialog.CustomPlaces.Add(new Microsoft.Win32.FileDialogCustomPlace(guess));
                }
            }

            if (openFileDialog.ShowDialog() == true)
            {
                InstallationLocations.Singleton.TryAdd(new InstallationLocation(openFileDialog.FileName));
            }
        }

        private static List<string> GenerateDcsRootDirectoryGuesses()
        {
            HashSet<string> existing = new HashSet<string>(
                InstallationLocations.Singleton.Items.Select(item => item.Path), 
                System.StringComparer.OrdinalIgnoreCase);
            string[] guessPaths = new string[] {
                System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "Eagle Dynamics", "DCS World"),
                System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "DCSWorld"),
                System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "DCSWorld.OpenBeta")
            };

            // generate a lot of guesses where it might be
            List<string> guesses = new List<string>();
            foreach (string guess in guessPaths)
            {
                guesses.Add(guess);
            }
            foreach (System.IO.DriveInfo drive in System.IO.DriveInfo.GetDrives())
            {
                if ((drive.DriveType == System.IO.DriveType.Fixed) && (drive.IsReady))
                {
                    if (drive.Name.Substring(1) == ":\\")
                    {
                        string letter = drive.Name.Substring(0, 1);

                        // check if it is a different drive
                        foreach (string guess in guessPaths)
                        {
                            if ((guess.Substring(1, 2) == ":\\") && (!guess.StartsWith(drive.Name)))
                            {
                                guesses.Add($"{letter}{guess.Substring(1)}");
                            }
                        }
                        // check the root too
                        guesses.Add($"{letter}:\\DCS");
                    }
                }
            }

            // now filter to existing directories we haven't already added
            return guesses.Where(guess => (!existing.Contains(guess)) && (System.IO.Directory.Exists(guess))).ToList();
        }

        private static void Remove_Executed(object target, ExecutedRoutedEventArgs e)
        {
            InstallationLocations.Singleton.TryRemove(e.Parameter as InstallationLocation);
        }
        #endregion

    }
}
