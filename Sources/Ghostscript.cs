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
using System.Diagnostics;
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

		public static void Convert(string format, string outputfile, params string[] inputfiles)
		{
			string commandline = String.Format("-dNOPAUSE -dBATCH -sDEVICE={0} -sOutputFile=\"{1}\"",
			                                   format, outputfile);
			foreach ( string file in inputfiles )
				commandline += String.Format(" \"{0}\"", file);

			Process gs = Process.Start(ExecutablePath, commandline);
			gs.WaitForExit();
		}

		public delegate void ConversionCompleted();
		public static void AsyncConvert(ConversionCompleted completed, string format, string outputfile,
		                                params string[] inputfiles)
		{
			Thread mythread = new Thread(new ThreadStart(delegate {
				Convert(format, outputfile, inputfiles);
				completed();
			}));

			mythread.Start();
		}
	}
}
