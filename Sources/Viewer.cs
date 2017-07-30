/*
 * This file is part of GSDocViewer library.
 * Copyright © 2009 Diego E. Pettenò <flameeyes@flameeyes.eu>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
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

		protected virtual void Clear()
		{
			imgDisplay.Pixbuf = null;
		}

		private int _page;
		public virtual int Page
		{
			get { return _page; }

			set {
				_page = value;

				goBackAction.Sensitive = (_page > 1);
				goForwardAction.Sensitive = (_page < _pages);
			}
		}

		private int _pages;
		public virtual int Pages
		{
			get { return _pages; }

			set {
				_pages = value;

				goBackAction.Sensitive = (_pages > 1);
				goForwardAction.Sensitive = (_pages > 1);
			}
		}

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
