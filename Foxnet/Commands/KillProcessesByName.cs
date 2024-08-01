using System.Linq;

using Hacknet;

using Pathfinder.Executable;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class KillProcessesByName: CommandBase {
	public override string[] Aliases { get; } = ["killall", "kills", "ka", "ks"];
	public override string Description { get; } = "Kills process with a name containing (default) or matching (-e) the given string";
	public override string[] Arguments { get; } = ["-e?", "search"];

	public override void Execute(OS os, string cmd, string[] args) {
		bool exact = false;
		string target = args[0].ToLower();
		if (args.Length == 2) {
			exact = args[0].ToLower().Equals("-e");
			target = args[1].ToLower();
		}
		bool found = false;
		//Dictionary<string, string> localExeNames = os.thisComputer.files.root.searchForFolder("bin").files.ToDictionary(f => f.data, f => f.name);
		foreach (ExeModule exe in os.exes.ToArray()) {
			string[] names = [
				exe.name ?? string.Empty,
				exe.IdentifierName ?? string.Empty,
				null!,
			];
			if (exe is BaseExecutable custom)
				names[2] = custom.Args[0];
			else
				names[2] = string.Empty; // TODO find a way to get this from base game exes
			string debug = $"PID {exe.PID}: name=[{names[0]}] IdentifierName=[{names[1]}] arg0=[{names[2]}] - ";
			if (names.Any(n => exact ? n.ToLower().Equals(target) : n.ToLower().Contains(target))) {
				Foxnet.Debug(debug + "MATCH");
				exe.Kill();
				Foxnet.Libsune.Terminal.Print($"Killed {exe.IdentifierName ?? exe.name} ({exe.PID})");
				found = true;
			}
			else {
				Foxnet.Debug(debug + "NO MATCH");
			}
		}
		if (!found) {
			Foxnet.Libsune.Terminal.Print("No matching processes found");
			if (exact)
				Foxnet.Libsune.Terminal.Print("Did you mean fuzzy mode? (Does not use -e)");
		}
	}
}
