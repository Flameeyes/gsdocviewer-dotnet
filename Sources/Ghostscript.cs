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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
			foreach ( string file in inputfiles ) {
				switch ( Path.GetExtension(file) ) {
				case ".pdf":
				case ".ps":
					break;
				default:
					throw new System.Exception(String.Format("Unable to handle file {0}", file));
				}
				commandline += String.Format(" \"{0}\"", file);
			}

			ProcessStartInfo sinfo = new ProcessStartInfo(ExecutablePath, commandline);
			sinfo.RedirectStandardOutput = true;
			sinfo.RedirectStandardError = true;
			sinfo.UseShellExecute = false;

			Process gs = Process.Start(sinfo);
			gs.WaitForExit();

			if ( gs.ExitCode != 0 )
				throw new System.Exception("Error executing conversion:\n" + gs.StandardOutput.ReadToEnd());

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

		public delegate void ConversionCompleted(string outputfile, int pages);

		private static void AsyncConvertInternal(ConversionCompleted completed, string format, string outputfile,
		                                        IEnumerable<string> inputfiles)
		{
			Thread mythread = new Thread(new ThreadStart(delegate {
				int pages = ConvertInternal(format, outputfile, inputfiles);
				completed(outputfile, pages);
			}));

			mythread.Start();
		}

		public static void AsyncConvert(ConversionCompleted completed, string format, string outputfile,
		                                params string[] inputfiles)
		{
			AsyncConvertInternal(completed, format, outputfile, inputfiles);
		}

		public static void AsyncConvert(ConversionCompleted completed, string format, string outputfile,
		                                IEnumerable<string> inputfiles)
		{
			AsyncConvertInternal(completed, format, outputfile, inputfiles);
		}
	}
}
