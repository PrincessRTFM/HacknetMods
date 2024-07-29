using System.Collections.Generic;
using System.Linq;

using Hacknet;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ShowPluginHelp: CommandBase {
	public override string Command { get; } = "foxnet";
	public override string Description { get; } = "Provides command help for all Foxnet commands";
	public override string[] Arguments { get; } = ["command?"];

	public override void Execute(OS os, string cmd, string[] args) {
		List<string> lines = [
			"\n"
		];

		if (args.Length == 1) {
			string want = args[0].ToLower();
			if (Foxnet.RegisteredCommands.TryGetValue(want, out CommandBase found)) {
				lines.Add(found.Usage);
				lines.AddRange(found.Description.Split('\n').Select(line => $"> {line}"));
			}
			else {
				os.Print("No Foxnet command exists with that name");
				return;
			}
		}
		else {
			lines.Add("=ouo= FOXNET HELP =ouo=");
			foreach (string key in Foxnet.RegisteredCommands.Keys) {
				CommandBase command = Foxnet.RegisteredCommands[key];
				if (command.Command.ToLower() == key)
					lines.Add(command.Usage);
			}
			lines.Add("=ouo= FOXNET HELP =ouo=");
		}

		lines.Add("\n");
		lines.AddRange(Foxnet.Snark);
		lines.Add("\n");

		foreach (string line in lines)
			os.Print(line);
	}
}
