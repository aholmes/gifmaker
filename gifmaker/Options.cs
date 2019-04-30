using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgParser.Core;
using ArgParser.Styles;
using ArgParser.Styles.Extensions;

namespace gifmaker
{
	public class Options
	{
		public static IParseResult Parse(string[] args)
		{
			return new ContextBuilder()
			.AddParser<Options>("GIF Maker", help =>
			{
				help
					.SetName("Gif Maker")
					.SetShortDescription("Make a GIF from a sequence of images.");
			})
			.WithFactoryFunction(() => new Options())
			.WithBooleanSwitch('h', "help", o => o.Help = true)
			.WithSingleValueSwitch('i', "input", (o, s) => o.InputPath = s, help =>
			 {
				 help
					 .SetName("Input Path")
					 .SetShortDescription("The directory containing images to sequence into a GIF.");
			 })
			.WithSingleValueSwitch('o', "output", (o, s) => o.OutputPath = s, help =>
			 {
				 help
					 .SetName("Input Path")
					 .SetShortDescription("The directory containing images to sequence into a GIF.");
			 })
			.WithSingleValueSwitch('d', "delay", (o, s) => o.FrameDelay = int.Parse(s), help =>
			{
				help
					.SetName("Frame Delay")
					.SetShortDescription("The delay, in ms, between each frame of the GIF.");
			})
			.Finish
			.RegisterExtensions()
			.AddAutoHelp((parseResults, exceptions) =>
			{
				foreach (var kvp in parseResults)
				{

					var casted = kvp.Key as Options;
					if (casted is null) continue;
					if (casted.Help)
						return kvp.Value.Id;
				}

				var missingValues = exceptions.OfType<MissingValueException>();
				var first = missingValues.FirstOrDefault();
				return first?.Parser.Id;
			})
			.Parse(args);
		}
		public string InputPath { get; set; }

		private string _outputPath;
		public string OutputPath
		{
			get
			{
				if (string.IsNullOrEmpty(_outputPath))
				{
					_outputPath = System.IO.Path.Combine(InputPath, "Output");
				}
				return _outputPath;
			}
			set { _outputPath = value; }
		}

		public int FrameDelay { get; set; }

		public bool Help { get; set; }
	}
}
