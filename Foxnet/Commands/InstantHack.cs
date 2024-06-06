using System.Collections.Generic;

using Hacknet;

using Pathfinder.Port;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class InstantHack: CommandBase {
	public override string Description { get; } = "Instantly hacks the connected computer, IF you have the portcrushers";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			List<PortState> ports = c.GetAllPortStates();

			if (c.GetRealPortsNeededForCrack() > ports.Count) {
				os.write("Cannot bypass: more ports are needed to crack than are available");
				return;
			}

			if (ports.Count > 0) {
				Folder bin = os.thisComputer.files.root.searchForFolder("bin");

				List<PortState> cannotOpen = [];
				List<PortState> canOpen = [];
				foreach (PortState port in ports) {
					if (port.Cracked) {
						canOpen.Add(port);
						continue;
					}

					if (
						PortExploits.crackExeData.TryGetValue(port.Record.OriginalPortNumber, out string exeData)
						|| PortExploits.crackExeData.TryGetValue(port.Record.DefaultPortNumber, out exeData)
					) {
						if (bin.containsFileWithData(exeData)) {
							canOpen.Add(port);
							continue;
						}
					}
					cannotOpen.Add(port);
				}

				if (canOpen.Count < c.GetRealPortsNeededForCrack()) {
					int needed = c.GetRealPortsNeededForCrack() - canOpen.Count;
					if (cannotOpen.Count > 1) {
						os.write("Cannot bypass: unable to open the following ports:");
						foreach (PortState blocked in cannotOpen)
							os.write($"- {blocked.DisplayName} ({blocked.Record.Protocol}, {blocked.PortNumber})");
					}
					else if (cannotOpen.Count == 1) {
						PortState blocked = cannotOpen[0];
						os.write($"Cannot bypass: unable to open {blocked.DisplayName} ({blocked.Record.Protocol}, {blocked.PortNumber})");
					}
					os.write($"At least {needed} more port{(needed == 1 ? "" : "s")} must be opened than you have crushers for");
					return;
				}
			}

			Foxnet.RegisteredCommands[nameof(ForceHack)].Execute(os, cmd, args);
		}
	}
}
