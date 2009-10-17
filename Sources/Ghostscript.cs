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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Portability;

namespace GSDocViewer
{
	public static class Ghostscript
	{
		private static string ExecutablePath;

		static Ghostscript()
		{
			switch(System.Environment.OSVersion.Platform)
			{
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				ExecutablePath = Paths.FindExecutable("gs");
				break;
			case PlatformID.Win32NT:
				ExecutablePath = System.IO.Path.Combine(Paths.ProgramDir, "ghostscript\\gs.exe");
				break;
			}
		}

		private static Regex counter = new Regex(@"Processing pages \d+ through (\d+)\.", RegexOptions.Compiled);

		private static int ConvertInternal(string format, string outputfile, IEnumerable<string> inputfiles)
		{
			string commandline = String.Format("-dNOPAUSE -dBATCH -sDEVICE={0} -sOutputFile=\"{1}\"",
			                                   format, outputfile);
			foreach ( string file in inputfiles )
				commandline += String.Format(" \"{0}\"", file);

			ProcessStartInfo sinfo = new ProcessStartInfo(ExecutablePath, commandline);
			sinfo.RedirectStandardOutput = true;
			sinfo.UseShellExecute = false;

			Process gs = Process.Start(sinfo);
			gs.WaitForExit();

			while ( !gs.StandardOutput.EndOfStream ) {
				string line = gs.StandardOutput.ReadLine();
				Match linem = counter.Match(line);

				if ( linem.Success )
					return System.Convert.ToInt32(linem.Groups[1].Value);
			}

			return -1;
		}

		public static int Convert(string format, string outputfile, params string[] inputfiles)
		{
			return ConvertInternal(format, outputfile, inputfiles);
		}

		public static int Convert(string format, string outputfile, IEnumerable<string> inputfiles)
		{
			return ConvertInternal(format, outputfile, inputfiles);
		}

		public delegate void ConversionCompleted(int pages);

		public static void AsyncConvert(ConversionCompleted completed, string format, string outputfile,
		                                params string[] inputfiles)
		{
			Thread mythread = new Thread(new ThreadStart(delegate {
				int pages = ConvertInternal(format, outputfile, inputfiles);
				completed(pages);
			}));

			mythread.Start();
		}
	}
}
