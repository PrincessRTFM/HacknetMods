using System;
using System.Collections.Generic;
using System.Linq;

using Hacknet;

using Pathfinder.Executable;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class GetAllPrograms: CommandBase {
	public override string[] Aliases { get; } = ["xmas"];
	public override string Description { get; } = "Gives you a copy of EVERY executable in the game";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		Folder bin = os.thisComputer.files.root.searchForFolder("bin");
		HashSet<string> progs = new(bin.files.Select(f => f.data));
		List<FileEntry> files = [];
		foreach (int exeNum in PortExploits.exeNums) {
			string exeName = PortExploits.cracks[exeNum];
			string magic = PortExploits.crackExeData[exeNum];

			if (!progs.Contains(magic))
				files.Add(new(magic, exeName));
		}
		try {
			foreach (ExecutableManager.CustomExeInfo ce in ExecutableManager.AllCustomExes) {
				if (!progs.Contains(ce.ExeData))
					files.Add(new(ce.ExeData, ce.ExeType.FullName + ".exe"));
			}
		}
		catch (Exception e) {
			Foxnet.Libsune.Terminal.Print($"Failed to reflect into custom executable manager");
			Foxnet.Libsune.Terminal.Print($"{e.GetType().Name}:\n{e.Message}");
			Foxnet.Libsune.Terminal.Print("Cannot provide custom executables");
		}
		bin.files.AddRange(files);
		Foxnet.Libsune.Terminal.Print($"Added {files.Count} program{(files.Count == 1 ? "" : "s")} to your /bin folder.");
		if (files.Count > 0 && cmd == "xmas")
			Foxnet.Libsune.Terminal.Print("Ho ho ho, ya naughty fuck.");
	}
}
