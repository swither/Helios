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

namespace GadrocsWorkshop.Helios
{
    using System.Windows.Media;

    public interface IImageManager
    {
        /// <summary>
        /// Loads an image file from the specified URI.
        /// </summary>
        /// <param name="uri">Can be a file system path, a pack URI, or a special uri of form {assembly}/relativePath</param>
        /// <returns></returns>
        ImageSource LoadImage(string uri);

        /// <summary>
        /// Loads an image file iterating through the profile subdirectories.
        /// </summary>
        /// <param name="uri">Can be a file system path, a pack URI, or a special uri of form {assembly}/relativePath</param>
        /// <returns></returns>
        ImageSource LoadImage(string uri, int width, int height);
        
        string MakeImagePathRelative(string filename);
        string MakeImagePathAbsolute(string fileName);
    }

    public class ImageLoadEventArgs : EventArgs
    {
        public ImageLoadEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }

    /// <summary>
    /// Version 2 of IImageManager interface
    /// </summary>
    public interface IImageManager2: IImageManager
    {
        event EventHandler<ImageLoadEventArgs> ImageLoadSuccess;
        event EventHandler<ImageLoadEventArgs> ImageLoadFailure;

        /// <summary>
        /// discard any previous image load failures; to be called when switching to a new profile
        /// or other context where previous failures would not be relevant to ReplayCurrentFailures
        /// </summary>
        void ClearFailureTracking();

        /// <summary>
        /// replays all failed image loads since the last call to ClearFailureTracking
        /// </summary>
        /// <param name="imageLoadFailureHandler"></param>
        void ReplayCurrentFailures(Action<object, ImageLoadEventArgs> imageLoadFailureHandler);
    }
}
