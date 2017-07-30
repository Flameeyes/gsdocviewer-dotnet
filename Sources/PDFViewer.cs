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
				Clear();

				if ( !File.Exists(value) )
					throw new FileNotFoundException("Unable to find PDF file", value);

				PDF_FullPath = value;
				PDF_BaseName = System.IO.Path.GetFileNameWithoutExtension(value);

				ConvertIf();
			}
		}

		protected void ConversionComplete(string outputfile, int pages)
		{
			Sensitive = true;
			Pages = pages;
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
				ConversionComplete(null, Directory.GetFiles(Paths.CacheFile("PDFViewer_Tiff", PDF_BaseName)).Length);
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
