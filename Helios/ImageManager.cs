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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using GadrocsWorkshop.Helios.Util;
using NLog;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// ImageManager gives access to loading and resizing images from the appropriate locations.
    /// </summary>
    public class ImageManager : IImageManager2
    {
        private readonly string _documentImagePath;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Uri _documentImageUri;
        private readonly HashSet<string> _failedImagePaths = new HashSet<string>();

        public event EventHandler<ImageLoadEventArgs> ImageLoadSuccess;
        public event EventHandler<ImageLoadEventArgs> ImageLoadFailure;

        private readonly XamlFirewall _xamlFirewall;

        internal ImageManager(string userImagePath)
        {
            Logger.Debug($"Helios will load user images from {Anonymizer.Anonymize(userImagePath)}");
            _documentImagePath = userImagePath;
            _documentImageUri = new Uri(userImagePath);
            _xamlFirewall = new XamlFirewall();
        }

        private ImageSource DoLoadImage(string uri, int? width, int? height)
        {
            // parse as URI and check for existence of file, return Uri for resource that exists
            // or null (note: resource is not necesarily allowed to be read)
            Uri imageUri = GetImageUri(uri);
            if (imageUri == null)
            {
                return null;
            }

            // based on protocol/scheme, check to make sure source location is permitted
            if (!CheckImageLocationSecurity(imageUri))
            {
                return null;
            }

            if (uri.EndsWith(".xaml"))
            {
                return imageUri.Scheme == "pack" ? LoadXamlResource(imageUri, width, height) : LoadXamlFile(imageUri, width, height);
            }

            Logger.Debug("image being loaded from {URI}", Anonymizer.Anonymize(imageUri));
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.CreateOptions = BitmapCreateOptions.DelayCreation;
            image.UriSource = imageUri;

            // REVISIT: not clear if it is legal to set decoding in just one axis but we will find out if anyone ever uses this function with only one scaling factor
            if (width.HasValue)
            {
                image.DecodePixelWidth = Math.Max(1, width.Value);
            }
            if (height.HasValue)
            {
                image.DecodePixelHeight = Math.Max(1, height.Value);
            }

            image.EndInit();
            return image;
        }

        private bool CheckImageLocationSecurity(Uri imageUri)
        {
            switch (imageUri.Scheme)
            {
                case "pack":
                    Logger.Debug("attempt to load image from application resource");

                    if (imageUri.Authority != "application:,,,")
                    {
                        Logger.Warn("siteoforigin or other pack authority denied for security reasons: {URI}",
                            Anonymizer.Anonymize(imageUri));
                        break;
                    }

                    if (string.IsNullOrEmpty(imageUri.AbsolutePath))
                    {
                        Logger.Warn("empty path cannot be resolved: {URI}", Anonymizer.Anonymize(imageUri));
                        break;
                    }

                    string[] components = imageUri.AbsolutePath.Split('/');
                    if (components.Length < 3 ||
                        components[0].Length > 0 ||
                        !components[1].Contains(";component"))
                    {
                        Logger.Warn("pack reference into file system disallowed for security reasons: {URI}",
                            Anonymizer.Anonymize(imageUri));
                        break;
                    }

                    switch (components[2])
                    {
                        case "Images":
                        case "Gauges":
                            return true;
                        case "Interfaces":
                            // it is possible that this was only ever used in a single BMS-centric template
                            if (imageUri.AbsolutePath.EndsWith(".png"))
                            {
                                return true;
                            }
                            break;
                    }
                    Logger.Warn(
                        "pack reference into assembly disallowed because it does not target 'Images' or 'Gauges' folder or a PNG file in 'Interfaces': {URI}",
                        Anonymizer.Anonymize(imageUri));
                    break;

                case "file":
                    Logger.Debug("attempt to load image from local file");

                    if (imageUri.AbsolutePath.Contains("..") ||
                        !imageUri.AbsolutePath.StartsWith(_documentImageUri.AbsolutePath))
                    {
                        Logger.Warn(
                            "images references must be contained in user image files location {Required}; {URI} disallowed",
                            Anonymizer.Anonymize(_documentImagePath),
                            Anonymizer.Anonymize(imageUri));
                        break;
                    }

                    return true;

                default:
                    // this must be disabled since XAML can do whatever it wants so a profile could download any arbitrary code
                    Logger.Warn("XAML network request denied for security reasons: {URI}",
                        Anonymizer.Anonymize(imageUri));
                    break;
            }

            return false;
        }

        private static ImageSource LoadXamlResource(Uri imageUri, int? width, int? height)
        {
            Logger.Debug("XAML being loaded as vector drawing from trusted resource {URI}", Anonymizer.Anonymize(imageUri));
            StreamResourceInfo streamResourceInfo = Application.GetResourceStream(imageUri);
            if (streamResourceInfo == null)
            {
                // these are supposed to be in our assembly, so log as error
                Logger.Error("could not resolve XAML image at {URI}", Anonymizer.Anonymize(imageUri));
                return null;
            }
            Stream xamlStream = streamResourceInfo.Stream;
            if (xamlStream == null)
            {
                // these are supposed to be in our assembly, so log as error
                Logger.Error("XAML image not found at {URI}", Anonymizer.Anonymize(imageUri));
                return null;
            }
            using (xamlStream)
            {
                Canvas canvas = (Canvas) XamlReader.Load(xamlStream);
                return RenderXaml(canvas, width, height);
            }
        }

        private ImageSource LoadXamlFile(Uri imageUri, int? width, int? height)
        {
            Logger.Debug("XAML being loaded as vector drawing from {URI}", Anonymizer.Anonymize(imageUri));
            using (Stream xamlStream = new FileStream(imageUri.AbsolutePath, FileMode.Open))
            {
                try
                {
                    Canvas canvas = _xamlFirewall.LoadXamlDefensively<Canvas>(xamlStream);
                    return RenderXaml(canvas, width, height);
                }
                catch (XamlFirewall.DisallowedElementException ex)
                {
                    Logger.Error("attempt to load XAML {URI} that did not contain simple drawing code denied. {Element} is not allowed.",
                        imageUri, ex.ElementName);
                    return null;
                }
            }
        }

        private static ImageSource RenderXaml(Canvas canvas, int? width, int? height)
        {
            int scaledWidth = width.HasValue ? Math.Max(1, width.Value) : (int) canvas.Width;
            int scaledHeight = height.HasValue ? Math.Max(1, height.Value) : (int) canvas.Height;
            RenderTargetBitmap render =
                new RenderTargetBitmap(scaledWidth, scaledHeight, 96d, 96d, PixelFormats.Pbgra32);
            if (width.HasValue || height.HasValue)
            {
                double scaleX = canvas.Width > 0 ? scaledWidth / canvas.Width : 1.0;
                double scaleY = canvas.Height > 0 ? scaledHeight / canvas.Height : 1.0;
                canvas.RenderTransform = new ScaleTransform(scaleX, scaleY);
            }

            canvas.Measure(new Size(canvas.Width, canvas.Height));
            canvas.Arrange(new Rect(new Size(canvas.Width, canvas.Height)));
            render.Render(canvas);
            return render;
        }

        private Uri GetImageUri(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // empty string is a normal case for unitialized image paths, and so is null,
                // so short circuit here
                return null;
            }

            if (path.StartsWith("pack:") || path.StartsWith("{"))
            {
                string packPath = MakeImagePathAbsolute(path);
                Uri imageUri = new Uri(packPath, UriKind.Absolute);

                if (CanOpenPackUri(imageUri))
                {
                    return imageUri;
                }
            }
            else if (path.StartsWith("http:", true, CultureInfo.CurrentCulture) ||
                     path.StartsWith("https:", true, CultureInfo.CurrentCulture) ||
                     path.StartsWith("ftp:", true, CultureInfo.CurrentCulture) ||
                     path.StartsWith("file:", true, CultureInfo.CurrentCulture))
            {
                return new Uri(path);
            }
            else
            {
                string filePath = MakeImagePathAbsolute(path);
                if (!string.IsNullOrEmpty(filePath))
                {
                    return new Uri("file://" + filePath);
                }
            }

            return null;
        }

        public string MakeImagePathRelative(string filename)
        {
            string newFilename = filename;

            if (filename.Length > 0)
            {
                if (filename.StartsWith("pack://application:,,,/"))
                {
                    int closingIndex = filename.IndexOf(";component", StringComparison.CurrentCulture);
                    if (closingIndex > -1)
                    {
                        string assembly = filename.Substring(23, closingIndex - 23);
                        newFilename = "{" + assembly + "}" + filename.Substring(closingIndex + 10);
                    }
                }
                else
                {
                    string fullFilename = Path.GetFullPath(filename);
                    if (fullFilename.StartsWith(ConfigManager.ImagePath, StringComparison.CurrentCulture))
                    {
                        newFilename = fullFilename.Substring(ConfigManager.ImagePath.Length + 1);
                    }
                }
            }

            return newFilename;
        }

        public string MakeImagePathAbsolute(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            if (fileName.StartsWith("pack:"))
            {
                return fileName;
            }

            if (Path.IsPathRooted(fileName))
            {
                // absolute file path, we can check this
                if (File.Exists(fileName))
                {
                    return fileName;
                }
            }
            else
            {
                if (fileName.StartsWith("{"))
                {
                    int closingIndex = fileName.IndexOf('}');
                    if (closingIndex > -1)
                    {
                        // reference to an assembly by name, convert to pack URI
                        string assembly = fileName.Substring(1, closingIndex - 1);
                        return "pack://application:,,,/" + assembly + ";component" +
                               fileName.Substring(closingIndex + 1);
                    }
                }

                // First check the users images directory
                string loadName = Path.Combine(_documentImagePath, fileName);
                if (File.Exists(loadName))
                {
                    return loadName;
                }

                Logger.Warn("referenced user image not found at {loadName}", Anonymizer.Anonymize(loadName));
            }

            return "";
        }

        public static bool CanOpenPackUri(Uri uri)
        {
            try
            {
                StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
                if (null == streamResourceInfo)
                {
                    // this is the normal case for a resource that is not found
                    return false;
                }
                Stream resourceStream = streamResourceInfo.Stream;
                if (resourceStream.Length < 1)
                {
                    resourceStream.Close();
                    return false;
                }
                resourceStream.Close();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error loading image from pack reference to assembly {URI}.", uri);
                return false;
            }
        }

        public ImageSource LoadImage(string uri)
        {
            ImageSource result = DoLoadImage(uri, null, null);
            OnImageLoad(uri, result);
            return result;
        }

        private void OnImageLoad(string path, ImageSource result)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (result == null)
            {
                _failedImagePaths.Add(path);
                ImageLoadFailure?.Invoke(this, new ImageLoadEventArgs(path));
            }
            else
            {
                _failedImagePaths.Remove(path);
                ImageLoadSuccess?.Invoke(this, new ImageLoadEventArgs(path));
            }
        }

        public ImageSource LoadImage(string uri, int width, int height)
        {
            ImageSource result = DoLoadImage(uri, width, height);
            OnImageLoad(uri, result);
            return result;
        }

        public void ClearFailureTracking()
        {
            _failedImagePaths.Clear();
        }

        public void ReplayCurrentFailures(Action<object, ImageLoadEventArgs> imageLoadFailureHandler)
        {
            foreach (string path in _failedImagePaths)
            {
                imageLoadFailureHandler.Invoke(this, new ImageLoadEventArgs(path));
            }
        }
    }
}