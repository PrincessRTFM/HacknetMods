using System.Collections.Generic;
using System.Linq;

using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet;

public class SlowPrinter {

	public readonly struct DelayedLine(double d, string t) {
		public readonly double Delay = d;
		public readonly string Text = t;
	}

	internal static IEnumerator<ActionDelayer.Condition> CreateSlowPrinter(OS os, params DelayedLine[] lines) {
		foreach (DelayedLine line in lines) {
			if (line.Delay >= 0)
				yield return ActionDelayer.Wait(line.Delay);
			if (line.Text is not null)
				os.write(line.Text);
		}
	}

	public static List<DelayedLine> ConstructUniformDelay(double delay, params string[] lines)
		=> lines
			.Select(text => new DelayedLine(delay, text))
			.ToList();

	public static void SlowPrint(OS os, params DelayedLine[] lines)
		=> os.delayer.PostAnimation(CreateSlowPrinter(os, lines));

	public static void SlowPrint(OS os, double delay, params string[] lines)
		=> os.delayer.PostAnimation(CreateSlowPrinter(os, ConstructUniformDelay(delay, lines).ToArray()));
}
