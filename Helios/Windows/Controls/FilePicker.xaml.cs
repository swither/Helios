//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    /// <summary>
    /// A picker for a Lua file, with optional initial guess where to get it
    /// </summary>
    public partial class FilePicker : UserControl
    {
        public FilePicker()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        /// initial folder or null
        /// </summary>
        public string FolderGuess
        {
            get => (string) GetValue(FolderGuessProperty);
            set => SetValue(FolderGuessProperty, value);
        }

        public static readonly DependencyProperty FolderGuessProperty =
            DependencyProperty.Register("FolderGuess", typeof(string), typeof(FilePicker),
                new PropertyMetadata(null));

        /// <summary>
        /// picker result full path
        /// </summary>
        public string SelectedFilePath
        {
            get => (string) GetValue(SelectedFilePathProperty);
            set => SetValue(SelectedFilePathProperty, value);
        }

        public static readonly DependencyProperty SelectedFilePathProperty =
            DependencyProperty.Register("SelectedFilePath", typeof(string), typeof(FilePicker),
                new UIPropertyMetadata(""));

        /// <summary>
        /// text to show in the picker text field
        /// </summary>
        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FilePicker), new PropertyMetadata(null));

        /// <summary>
        /// the human-readable short description of the type of file being selected
        /// </summary>
        public string FileDescription
        {
            get => (string) GetValue(FileDescriptionProperty);
            set => SetValue(FileDescriptionProperty, value);
        }

        public static readonly DependencyProperty FileDescriptionProperty =
            DependencyProperty.Register("FileDescription", typeof(string), typeof(FilePicker),
                new PropertyMetadata("File"));

        /// <summary>
        /// extension to enforce on files being selected, not including the dot
        /// </summary>
        public string FileExtension
        {
            get => (string) GetValue(FileExtensionProperty);
            set => SetValue(FileExtensionProperty, value);
        }

        public static readonly DependencyProperty FileExtensionProperty =
            DependencyProperty.Register("FileExtension", typeof(string), typeof(FilePicker),
                new PropertyMetadata("lua"));

        #endregion

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedFilePath = null;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                Multiselect = false,
                ValidateNames = true,
                Filter = $"{FileDescription} (*.{FileExtension})|*.{FileExtension}",
                Title = "Select {FileDescription}",
                InitialDirectory = FolderGuess ?? Environment.CurrentDirectory
            };

            bool? result = openFileDialog.ShowDialog(Window.GetWindow(this));
            if (result == true)
            {
                SelectedFilePath = openFileDialog.FileName;
            }
        }
    }
}