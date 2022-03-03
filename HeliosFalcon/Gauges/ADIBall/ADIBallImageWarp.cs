//  Copyright 2014 Craig Courtney
//  Copyright 2022 Helios Contributors
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GadrocsWorkshop.Helios.Gauges.Falcon.ADIBallRenderer
{
	public class ADIBallImageWarp
	{
		private byte[] ImageBytes;
		private int RowSizeBytes;
		private const int PixelDataSize = 32;
		private BitmapData m_BitmapData;
		public Bitmap Bitmap;

		public ADIBallImageWarp(Bitmap bitmap)
		{
			Bitmap = bitmap;
		}

		public ADIBallImageWarp WarpBallImage()
		{
			ADIBallImageWarp result = Clone();

			LockBitmap();
			result.LockBitmap();

			WarpImage(this, result);

			result.UnlockBitmap();

			return result;
		}

		#region Methods

		private static void WarpImage(ADIBallImageWarp bm_src, ADIBallImageWarp bm_dest)
		{
			double xmid = bm_dest.Width / 2.0;
			double ymid = bm_dest.Height / 2.0;
			double rmax = bm_dest.Width * 0.75;

			int ix_max = bm_src.Width - 2;
			int iy_max = bm_src.Height - 2;

			double x0, y0;

			for (int y1 = 0; y1 < bm_dest.Height; y1++)
			{
				for (int x1 = 0; x1 < bm_dest.Width; x1++)
				{
					MapPixel(xmid, ymid, rmax, x1, y1, out x0, out y0);

					int ix0 = (int)x0;
					int iy0 = (int)y0;

					if ((ix0 < 0) || (ix0 > ix_max) || (iy0 < 0) || (iy0 > iy_max))
					{
						bm_dest.SetPixel(x1, y1, 255, 255, 255, 255);
					}
					else
					{
						double dx0 = x0 - ix0;
						double dy0 = y0 - iy0;
						double dx1 = 1 - dx0;
						double dy1 = 1 - dy0;

						byte r00, g00, b00, a00, r01, g01, b01, a01, r10, g10, b10, a10, r11, g11, b11, a11;
						bm_src.GetPixel(ix0, iy0, out r00, out g00, out b00, out a00);
						bm_src.GetPixel(ix0, iy0 + 1, out r01, out g01, out b01, out a01);
						bm_src.GetPixel(ix0 + 1, iy0, out r10, out g10, out b10, out a10);
						bm_src.GetPixel(ix0 + 1, iy0 + 1, out r11, out g11, out b11, out a11);

						int r = (int)(r00 * dx1 * dy1 + r01 * dx1 * dy0 + r10 * dx0 * dy1 + r11 * dx0 * dy0);
						int g = (int)(g00 * dx1 * dy1 + g01 * dx1 * dy0 + g10 * dx0 * dy1 + g11 * dx0 * dy0);
						int b = (int)(b00 * dx1 * dy1 + b01 * dx1 * dy0 + b10 * dx0 * dy1 + b11 * dx0 * dy0);
						int a = (int)(a00 * dx1 * dy1 + a01 * dx1 * dy0 + a10 * dx0 * dy1 + a11 * dx0 * dy0);

						bm_dest.SetPixel(x1, y1, (byte)r, (byte)g, (byte)b, (byte)a);
					}
				}
			}
		}

		private static void MapPixel(double xmid, double ymid, double rmax, int x1, int y1, out double x0, out double y0)
		{
			double dx, dy, r1, r2;
			double warp_correction_factor = 1.6;
			double width_correction_factor = 1.5;
			double height_correction_factor = 1.0;

			dx = x1 - xmid;
			dy = y1 - ymid;

			r1 = Math.Pow(Math.Pow(dx / warp_correction_factor, 2) + Math.Pow(dy, 2), 0.45);

			if (r1 == 0)
			{
				x0 = xmid;
				y0 = ymid;
			}
			else
			{
				r2 = rmax / 2 * (1 / (1 - r1 / rmax) - 1);
				x0 = dx * r2 * width_correction_factor / r1 + xmid;
				y0 = dy * r2 * height_correction_factor / r1 + ymid;
			}
		}

		#endregion Methods

		#region Functions

		public void GetPixel(int x, int y, out byte red, out byte green, out byte blue, out byte alpha)
		{
			int i = y * m_BitmapData.Stride + x * 4;
			blue = ImageBytes[i++];
			green = ImageBytes[i++];
			red = ImageBytes[i++];
			alpha = ImageBytes[i];
		}

		public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha)
		{
			int i = y * m_BitmapData.Stride + x * 4;
			ImageBytes[i++] = blue;
			ImageBytes[i++] = green;
			ImageBytes[i++] = red;
			ImageBytes[i] = alpha;
		}

		public void LockBitmap()
		{
			if (IsLocked) return;

			Rectangle bounds = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
			m_BitmapData = Bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			RowSizeBytes = m_BitmapData.Stride;

			int total_size = m_BitmapData.Stride * m_BitmapData.Height;
			ImageBytes = new byte[total_size];

			Marshal.Copy(m_BitmapData.Scan0, ImageBytes, 0, total_size);

			IsLocked = true;
		}

		public void UnlockBitmap()
		{
			if (!IsLocked) return;

			int total_size = m_BitmapData.Stride * m_BitmapData.Height;
			Marshal.Copy(ImageBytes, 0, m_BitmapData.Scan0, total_size);

			Bitmap.UnlockBits(m_BitmapData);

			ImageBytes = null;
			m_BitmapData = null;

			IsLocked = false;
		}

		public ADIBallImageWarp Clone()
		{
			bool was_locked = IsLocked;

			LockBitmap();

			ADIBallImageWarp result = (ADIBallImageWarp)MemberwiseClone();

			result.Bitmap = new Bitmap(Bitmap.Width, Bitmap.Height);
			result.IsLocked = false;

			if (!was_locked) UnlockBitmap();

			return result;
		}

		#endregion Functions

		#region Properties

		private bool IsLocked { get; set; }

		private int Width
		{
			get
			{
				return Bitmap.Width;
			}
		}

		private int Height
		{
			get
			{
				return Bitmap.Height;
			}
		}

		#endregion Properties
	}
}
