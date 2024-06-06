using System.Collections.Generic;
using System.Linq;

using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class PrintFunnyComment: CommandBase {
	public override string Command { get; } = "foxlol";
	public override string Description { get; } = "Displays a silly/funny comment";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		List<SlowPrinter.DelayedLine> lines = [];
		string[] taglines = Foxnet.Snark.Split('\n');

		lines.Add(new(0, $"// {taglines[0]}"));

		if (taglines.Length > 1) {
			foreach (string tagline in taglines.Skip(1))
				lines.Add(new(1, $"// {tagline}"));
		}

		lines.Add(new(0.5, "\n"));

		SlowPrinter.SlowPrint(os, lines.ToArray());
	}
}
