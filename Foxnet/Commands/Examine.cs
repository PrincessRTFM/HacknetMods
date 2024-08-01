using System.Collections.Generic;

using Hacknet;

using Pathfinder.Port;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class Examine: CommandBase {
	public override string Description { get; } = "Displays a lot of information about the target computer";
	public override string[] Arguments { get; } = ["target?"];

	public override void Execute(OS os, string cmd, string[] args) {
		const string none = "[n/a]";

		Computer c = args.Length > 0
			? Programs.getComputer(os, args[0])
			: os.connectedComp ?? os.thisComputer;

		if (c is not null) {
			Administrator admin = c.admin;
			string owner = c.adminIP;
			string pass = c.adminPass;
			Firewall firewall = c.firewall;
			List<PortState> ports = c.GetAllPortStates() ?? [];
			bool proxied = c.hasProxy;
			bool tracked = c.HasTracker;
			string id = c.idName;
			string ip = c.ip;
			MemoryContents memory = c.Memory;
			string name = c.name;
			bool isAdmin = c.PlayerHasAdminPermissions();
			int needed = c.GetRealPortsNeededForCrack();
			bool proxyLive = c.proxyActive;
			float proxyTimeLeft = c.proxyOverloadTicks;
			int security = c.securityLevel;
			float proxyTime = c.startingOverloadTicks;
			float traceTime = c.traceTime;

			int memCommands = memory?.CommandsRun?.Count ?? 0;
			int memBlocks = memory?.DataBlocks?.Count ?? 0;
			int memFiles = memory?.FileFragments?.Count ?? 0;
			int memImages = memory?.Images?.Count ?? 0;

			string adminType = admin is not null
				? admin.GetType().Name
					+ " ["
					+ (admin?.IsSuper ?? false ? "super" : "normal")
					+ "] ["
					+ (admin?.ResetsPassword ?? false ? "" : "no")
					+ "reset]"
				: none;
			string firewallDesc = firewall is not null
				? "lv"
					+ firewall.complexity
					+ " (+"
					+ firewall.additionalDelay
					+ "s) "
					+ (firewall.solved ? "" : "un")
					+ "solved: "
					+ firewall.solution
				: none;
			string proxyDesc = proxied
				? (proxyLive ? "" : "in")
					+ "active, "
					+ proxyTimeLeft
					+ "/"
					+ proxyTime
				: none;
			string portDesc = ports.Count
				+ " port"
				+ (ports.Count == 1 ? "" : "s")
				+ " present";
			string crackDesc = needed
				+ " port"
				+ (needed == 1 ? "" : "s")
				+ " needed to crack";
			string memDesc = memory is null
				? none
				: memCommands
					+ " command"
					+ (memCommands == 1 ? "" : "s")
					+ ", "
					+ memBlocks
					+ " block"
					+ (memBlocks == 1 ? "" : "s")
					+ ", "
					+ memFiles
					+ " file"
					+ (memFiles == 1 ? "" : "s")
					+ ", "
					+ memImages
					+ " image"
					+ (memImages == 1 ? "" : "s");

			if (string.IsNullOrEmpty(owner))
				owner = none;
			if (string.IsNullOrEmpty(pass))
				pass = none;
			if (string.IsNullOrEmpty(id))
				id = none;
			if (string.IsNullOrEmpty(ip))
				ip = none;
			if (string.IsNullOrEmpty(name))
				name = none;

			List<string> lines = new([
				$"State of {ip}/{id}: {name}",
				$"Current admin: {owner} ({pass})",
				$"Admin access: {(isAdmin ? "" : "not ")}available",
				$"Sysadmin: {adminType}",
				$"Security level: {security}",
				$"Proxy: {proxyDesc}",
				$"Firewall: {firewallDesc}",
				portDesc,
				crackDesc
			]);

			if (ports.Count < needed) {
				lines.Add("This computer cannot be hacked!");
			}
			else if (ports.Count == 0) {
				lines.Add("This computer can always be hacked.");
			}
			else if (ports.Count > 0) {
				Folder bin = os.thisComputer.files.root.searchForFolder("bin");

				int canOpen = 0;
				foreach (PortState port in ports) {
					if (port.Cracked) {
						++canOpen;
						continue;
					}

					if (
						PortExploits.crackExeData.TryGetValue(port.Record.OriginalPortNumber, out string exeData)
						|| PortExploits.crackExeData.TryGetValue(port.Record.DefaultPortNumber, out exeData)
					) {
						if (bin.containsFileWithData(exeData)) {
							++canOpen;
							continue;
						}
					}
				}

				if (canOpen < c.GetRealPortsNeededForCrack()) {
					int missing = c.GetRealPortsNeededForCrack() - canOpen;
					lines.Add($"You cannot currently hack this computer.");
					lines.Add($"You must open {(missing == 1 ? "1 port" : $"{missing} ports")} more than you have tools for.");
				}
			}

			if (traceTime > 0)
				lines.Add($"Trace time: {traceTime}s");
			else
				lines.Add("No trace present");

			if (tracked)
				lines.Add("Tracker present");
			else
				lines.Add("No tracker present");

			lines.Add($"System memory: {memDesc}");

			lines.Add("\n");
			lines.AddRange(Foxnet.Snark);
			lines.Add("\n");

			Foxnet.Libsune.Terminal.Print(lines.ToArray());
		}
		else {
			Foxnet.Libsune.Terminal.Print("Target computer not found");
		}
	}
}
