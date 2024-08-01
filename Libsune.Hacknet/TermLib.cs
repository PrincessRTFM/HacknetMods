using System.Linq;

using BepInEx.Hacknet;

using Hacknet;

namespace PrincessRTFM.Hacknet.Lib;

public class TermLib {
	private readonly string pluginName;

	private string? termWritePrefix = null;
	public string OutputPrefix {
		get => this.termWritePrefix ?? $"[{this.pluginName}]";
		set => this.termWritePrefix = value.Trim();
	}

	internal TermLib(HacknetPlugin plugin) {
		plugin.Log.LogDebug($"{LibsuneHN.LogTag} Initialising terminal library");
		this.pluginName = plugin.GetType().Name;
	}

	public void Print(params string[] lines) => this.Print(OS.currentInstance, lines);
	public void Print(OS os, params string[] lines) {
		os ??= OS.currentInstance;
		if (os is null)
			return;

		foreach (string line in lines.SelectMany(s => s.Split(Utils.newlineDelim)))
			os.write($"{this.termWritePrefix} {line}".Trim());
	}
}
