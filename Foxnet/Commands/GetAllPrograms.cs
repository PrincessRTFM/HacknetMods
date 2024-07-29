using System;
using System.Collections.Generic;
using System.Linq;

using Hacknet;

using PrincessRTFM.Hacknet.Foxnet.Util;
using PrincessRTFM.Hacknet.Lib;

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
			foreach (RegisteredCustomExe ce in RegisteredCustomExe.GetCustomExes()) {
				if (!progs.Contains(ce.Magic))
					files.Add(new(ce.Magic, ce.QualifiedTypeName + ".exe"));
			}
		}
		catch (Exception e) {
			os.Print($"Failed to reflect into custom executable manager");
			os.Print($"{e.GetType().Name}:\n{e.Message}");
			os.Print("Cannot provide custom executables");
		}
		bin.files.AddRange(files);
		os.Print($"Added {files.Count} program{(files.Count == 1 ? "" : "s")} to your /bin folder.");
		if (files.Count > 0 && cmd == "xmas")
			os.Print("Ho ho ho, ya naughty fuck.");
	}
}
