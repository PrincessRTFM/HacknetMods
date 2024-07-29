using System.Linq;
using System.Reflection;

using Hacknet;

namespace PrincessRTFM.Hacknet.Lib;

public static class TermLib {
	private static string? termWritePrefix = null;
	public static string TerminalOutputPrefix {
		get => termWritePrefix ?? Assembly.GetExecutingAssembly().GetName().Name;
		set => termWritePrefix = value.Trim();
	}
	public static void Print(params string[] lines) => OS.currentInstance.Print(lines);

	public static void Print(this OS os, params string[] lines) {
		os ??= OS.currentInstance;
		if (os is null)
			return;

		foreach (string line in lines.SelectMany(s => s.Split(Utils.newlineDelim)))
			os.write($"{termWritePrefix} {line}".Trim());
	}

}
