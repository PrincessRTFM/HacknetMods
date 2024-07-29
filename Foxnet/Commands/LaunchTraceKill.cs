using System.Collections.Generic;
using System.Linq;

using Hacknet;

using Pathfinder.Executable;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class LaunchTraceKill: CommandBase {
	public const int TRACEKILL_RAM_COST = 600;

	public override string[] Aliases { get; } = ["tkill", "tk"];
	public override string Description { get; } = "Instantly launches TraceKill.exe, closing all open programs if you don't have the RAM";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		const string name = "TraceKill.exe";
		string magic = Foxnet.GetMagicFromExeName(name);

		if (!string.IsNullOrEmpty(magic)) {
			int free = os.ramAvaliable;
			IEnumerable<ExeModule> liveExes = os.exes.Where(e => e.ramCost > 0);
			if (free < TRACEKILL_RAM_COST) { // PASS ONE - remove notes
				ExeModule[] notes = liveExes.Where(e => e is NotesExe).ToArray();
				foreach (ExeModule exe in notes) {
					int ram = exe.ramCost;
					if (exe.Kill())
						free += ram;
					if (free >= TRACEKILL_RAM_COST)
						break;
				}
			}
			if (free < TRACEKILL_RAM_COST) { // PASS TWO - remove shells
				ExeModule[] shells = liveExes.Where(e => e is ShellExe).ToArray();
				foreach (ExeModule exe in shells) {
					int ram = exe.ramCost;
					if (exe.Kill())
						free += ram;
					if (free >= TRACEKILL_RAM_COST)
						break;
				}
			}
			if (free < TRACEKILL_RAM_COST) { // PASS THREE - remove everything that's left
				ExeModule[] programs = liveExes.ToArray();
				foreach (ExeModule exe in programs) {
					int ram = exe.ramCost;
					if (exe.Kill())
						free += ram;
					if (free > TRACEKILL_RAM_COST)
						break;
				}
			}
			os.ramAvaliable = free;
			os.launchExecutable(name, magic, 12); // target port isn't even used in the method, wtf
		}
		else {
			os.Print($"Foxnet error: cannot find internal data for {name}");
		}
	}
}
