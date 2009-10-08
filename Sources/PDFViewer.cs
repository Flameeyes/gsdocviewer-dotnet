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
using System.IO;
using Portability;

namespace GSDocViewer
{
	[System.ComponentModel.ToolboxItem(true)]
	public class PDFViewer : Viewer
	{
		public PDFViewer()
			: base()
		{
		}

		protected string PDF_FullPath;
		protected string PDF_BaseName;

		public string PDF
		{
			get
			{
				return PDF_FullPath;
			}

			set
			{
				if ( !File.Exists(value) )
					throw new FileNotFoundException("Unable to find PDF file", value);

				PDF_FullPath = value;
				PDF_BaseName = System.IO.Path.GetFileNameWithoutExtension(value);

				ConvertIf();
			}
		}

		protected void ConversionComplete()
		{
			Sensitive = true;
			Page = 1;
		}

		protected void ConvertIf()
		{
			if ( !File.Exists(Paths.CacheFile("PDFViewer_Tiff",
			                                  PDF_BaseName,
			                                  "0001.tiff")) ) {
				Sensitive = false;
				Ghostscript.AsyncConvert(ConversionComplete,
				                         "tiffg3",
				                         Paths.CacheFile("PDFViewer_Tiff", PDF_BaseName, "%04d.tiff"),
				                         PDF_FullPath);
			} else {
				ConversionComplete();
			}
		}

		public override int Page {
			get {
				return base.Page;
			}
			set {
				if ( PDF_BaseName == null )
					return;

				string page_path = Paths.CacheFile("PDFViewer_Tiff",
				                                   PDF_BaseName,
				                                   String.Format("{0:d4}.tiff", value));

				if ( !File.Exists(page_path) )
					throw new OverflowException("Page not found");

				OpenFile(page_path);
				base.Page = value;
			}
		}
	}
}
