using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;

namespace gifmaker
{
	class Program
	{
		static void Main(string[] args)
		{
			Options options = null;
			try
			{
				var parserResult = Options.Parse(args);

				parserResult.When<Options>((o, p) => { options = o; });

				if (options == null || options.Help)
				{
					Exit();
				}

				var progress = new Progress<GifWriterProgress>();
				progress.ProgressChanged += (s,e) =>
				{
					var outputStr = $"{e.Text}: {e.ProgressPercentage}%";
					var lineStr = new string(' ', Console.WindowWidth - outputStr.Length-1);
					Console.Write($"\r{outputStr}{lineStr}");
				};

				var gifFilename = new GifWriter
				{
					InputDir = options.InputPath,
					OutputDir = options.OutputPath,
					FrameDelay = options.FrameDelay
				}.Execute(progress);

				Console.WriteLine($"\nFile written to \"{gifFilename}\"");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

			Exit();
		}

		private static void Exit()
		{
			Console.Write("\nPress return to quit ...");
			Console.ReadLine();

			Environment.Exit(0);
		}
	}

	public class GifWriterProgress
	{
		public int ProgressPercentage { get; set; }
		public string Text { get; set; }
	}

	public class GifWriter
	{
		public string InputDir { get; set; }
		public string OutputDir { get; set; }
		public int FrameDelay { get; set; }

		private int _totalProgressSteps;

		public string Execute(IProgress<GifWriterProgress> progress = null)
		{
			if (!Directory.Exists(InputDir)) throw new Exception($"Directory \"{InputDir}\" does not exist.");

			var files = Directory.EnumerateFiles(InputDir, "*.jpg", SearchOption.TopDirectoryOnly).ToArray();

			if (!files.Any()) throw new Exception($"No files ending in \".jpg\" found in directory \"{InputDir}\"");

			using (var collection = new MagickImageCollection())
			{
				_totalProgressSteps = files.Length + 4;
				var steps = 0;
				foreach (var file in files)
				{
					Report(progress, ++steps, "Add File");
					collection.Add(file);
					collection[0].AnimationDelay = FrameDelay;
				}

				QuantizeSettings settings = new QuantizeSettings
				{
					Colors = 256
				};
				Report(progress, ++steps, "Quantize");
				collection.Quantize(settings);

				Report(progress, ++steps, "Optimize");
				collection.Optimize();

				Report(progress, ++steps, "Create Dir");
				Directory.CreateDirectory(OutputDir);

				var filename = Path.Combine(OutputDir, $"out-{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}.gif");

				Report(progress, ++steps, "Write File");
				collection.Write(filename);

				return filename;
			}
		}

		private void Report(IProgress<GifWriterProgress> progress, int step, string text)
		{
			if (progress == null) return;

			var args = new GifWriterProgress
			{
				ProgressPercentage = 100 * step / _totalProgressSteps,
				Text = text
			};
			progress.Report(args);
		}
	}
}
