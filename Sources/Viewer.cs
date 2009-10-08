/*
 * This file is part of GSDocViewer library.
 * Copyright © 2009 Diego E. Pettenò <flameeyes@gmail.com>
 *
 * GSDocViewer is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * GSDocViewer is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Portability.
 * If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace GSDocViewer
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class Viewer : Gtk.Bin
	{
		protected Gdk.Pixbuf fileBuf;
		public enum ScaleMethod {
			FitWidth,
			FitHeight,
			FitBoth
		};

		protected double ScaleRatio;

		public Viewer()
		{
			this.Build();
			imgDisplay.SetSizeRequest(1, 1);
			imgDisplay.SizeRequest();
		}

		public void OpenFile(string path)
		{
			SetPixbuf(new Gdk.Pixbuf(path));
		}

		public void SetPixbuf(Gdk.Pixbuf img)
		{
			fileBuf = img;
			Scale(ScaleMethod.FitWidth);
			RedrawImage();
		}

		protected void Scale(ScaleMethod method)
		{
			switch (method)
			{
			case ScaleMethod.FitHeight:
				ScaleRatio = (double)(imgDisplay.Allocation.Height)/fileBuf.Height;
				break;
			case ScaleMethod.FitWidth:
				ScaleRatio = (double)(imgDisplay.Allocation.Width)/fileBuf.Width;
				break;
			case ScaleMethod.FitBoth:
				double ratioWidth  = (double)(imgDisplay.Allocation.Width)/fileBuf.Width;
				double ratioHeight = (double)(imgDisplay.Allocation.Height)/fileBuf.Height;

				if ( ratioWidth < ratioHeight )
					ScaleRatio = ratioWidth < 1 ? ratioWidth : ratioHeight;
				else
					ScaleRatio = ratioHeight < 1 ? ratioHeight : ratioWidth;
				break;
			default:
				throw new Exception();
			}

		}

		protected void RedrawImage()
		{
			int displayWidth = (int)(ScaleRatio * fileBuf.Width);
			int displayHeight = (int)(ScaleRatio * fileBuf.Height);

			Gdk.Pixbuf displayBuf = new Gdk.Pixbuf(fileBuf.Colorspace, fileBuf.HasAlpha,
			                                       fileBuf.BitsPerSample,
			                                       displayWidth, displayHeight);

			fileBuf.Scale(displayBuf,
			              0, 0, displayWidth, displayHeight, 0, 0,
			              ScaleRatio, ScaleRatio,
			              Gdk.InterpType.Bilinear);

			imgDisplay.Pixbuf = displayBuf;
			imgDisplay.SetSizeRequest(displayWidth, displayHeight);
		}

		protected virtual void OnZoomFitActionActivated (object sender, System.EventArgs e)
		{
			imgDisplay.Pixbuf = null;
			imgDisplay.SetSizeRequest(1, 1);

			Gtk.Application.RunIteration();

			Scale(ScaleMethod.FitBoth);
			RedrawImage();
		}

		protected virtual void OnZoom100ActionActivated (object sender, System.EventArgs e)
		{
			ScaleRatio = 1.0;
			RedrawImage();
		}

		protected virtual void OnZoomInActionActivated (object sender, System.EventArgs e)
		{
			ScaleRatio *= 1.1;
			RedrawImage();
		}

		protected virtual void OnZoomOutActionActivated (object sender, System.EventArgs e)
		{
			ScaleRatio /= 1.1;
			RedrawImage();
		}

		protected virtual void OnClearActionActivated (object sender, System.EventArgs e)
		{
			imgDisplay.Pixbuf = null;
		}

		public virtual int Page
		{ get; set; }

		protected virtual void OnGoBackActionActivated (object sender, System.EventArgs e)
		{
			try {
				Page = (Page-1);
			} catch(Exception) {
			}
		}

		protected virtual void OnGoForwardActionActivated (object sender, System.EventArgs e)
		{
			try {
				Page = (Page+1);
			} catch(Exception) {
			}
		}

	}
}
