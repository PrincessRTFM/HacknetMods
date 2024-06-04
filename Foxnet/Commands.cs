using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Hacknet;

using Pathfinder.Executable;
using Pathfinder.Port;

namespace PrincessRTFM.Hacknet.Foxnet;

[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Reflection 'n' shit")]
internal class Commands {

	[Command("foxnet")]
	[Description("Provides command help for all Foxnet commands")]
	[Arguments("command?")]
	public static void PluginHelp(OS os, string cmd, string[] args) {
		List<string> lines = new() {
			"\n"
		};

		if (args.Length == 1) {
			string want = args[0].ToLower();
			if (Foxnet.commands.TryGetValue(want, out CommandData found)) {
				lines.Add(found.Usage);
				lines.AddRange(found.Description.Split('\n').Select(line => $"> {line}"));
			}
			else {
				os.write("No Foxnet command exists with that name");
				return;
			}
		}
		else {
			lines.Add("=ouo= FOXNET HELP =ouo=");
			foreach (string key in Foxnet.commands.Keys) {
				CommandData command = Foxnet.commands[key];
				if (command.PrettyName.ToLower() == key)
					lines.Add(command.Usage);
			}
			lines.Add("=ouo= FOXNET HELP =ouo=");
		}

		lines.Add("\n");
		lines.AddRange(Foxnet.Snark.Split('\n').Select(line => $"// {line}"));
		lines.Add("\n");

		foreach (string line in lines)
			os.write(line);
	}

	#region Firewalls

	[Command("addFirewall")]
	[Description("Adds a firewall to the connected system")]
	[Arguments("complexity?", "solution?", "extra delay?")]
	public static void AddFirewall(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is null) {
				int level = 0;
				string pass = "";
				float extra = 0;
				if (args.Length >= 1)
					int.TryParse(args[0], out level);
				if (args.Length >= 2)
					pass = args[1].Trim();
				if (args.Length >= 3)
					float.TryParse(args[0], out extra);
				Firewall f = new(level);
				if (!string.IsNullOrEmpty(pass)) {
					f.solution = pass;
					f.solutionLength = pass.Length;
				}
				f.additionalDelay = extra;
				c.firewall = f;
				os.write($"Firewall added ({c.firewall.solution ?? "<no solution?>"})");
			}
			else {
				os.write("Firewall already present");
			}
		}
	}

	[Command("removeFirewall", "delFirewall")]
	[Description("Removes a firewall from the connected system")]
	[Arguments()]
	public static void RemoveFirewall(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				c.firewall = null;
				os.write($"Firewall removed");
			}
			else {
				os.write("No firewall present");
			}
		}
	}

	[Command("solveFirewall", "freeze")]
	[Description("Instantly solves the firewall on the connected system")]
	[Arguments()]
	public static void SolveFirewall(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				c.firewall.solved = true;
				c.firewall.analysisPasses = c.firewall.solutionLength;
				os.write($"Firewall unlocked ({c.firewall.solution ?? "<no solution?>"})");
			}
			else {
				os.write("No firewall present");
			}
		}
	}

	[Command("resetFirewall", "thaw")]
	[Description("Resets the firewall on the connected system")]
	[Arguments()]
	public static void LockFirewall(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				c.firewall.solved = false;
				c.firewall.resetSolutionProgress();
				os.write($"Firewall locked ({c.firewall.solution ?? "<no solution?>"})");
			}
			else {
				os.write("No firewall present");
			}
		}
	}

	[Command("setFirewallPassword", "setpass")]
	[Description("Changes the password for the firewall on the connected system")]
	[Arguments("password")]
	public static void SetFirewallPassword(OS os, string cmd, string[] args) {

		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				string pass = args[0];
				if (pass.Length > Firewall.MIN_SOLUTION_LENGTH) {
					c.firewall.solution = args[0];
					c.firewall.solutionLength = args[0].Length;
					os.write($"Firewall password changed to {c.firewall.solution ?? "<no solution?>"}");
				}
				else {
					os.write($"Firewall password must be more than {Firewall.MIN_SOLUTION_LENGTH} characters");
				}
			}
			else {
				os.write("No firewall present");
			}
		}
	}

	[Command("resetFirewallPassword", "randpass")]
	[Description("Randomises the password for the firewall on the connected system")]
	[Arguments("length?")]
	public static void SetRandomFirewallPassword(OS os, string cmd, string[] args) {

		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				if (args.Length < 1 || !int.TryParse(args[0], out int len))
					len = 0;
				if (len > Firewall.MIN_SOLUTION_LENGTH)
					c.firewall.solutionLength = len;
				c.firewall.generateRandomSolution();
				os.write($"Firewall password changed to {c.firewall.solution ?? "<no solution?>"}");
			}
			else {
				os.write("No firewall present");
			}
		}
	}

	#endregion

	#region Proxies

	[Command("addProxy")]
	[Description("Adds a proxy to the connected system")]
	[Arguments("strength")]
	public static void CreateProxy(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (!c.hasProxy) {
				if (float.TryParse(args[0], out float ticks)) {
					c.hasProxy = c.proxyActive = true;
					c.startingOverloadTicks = c.proxyOverloadTicks = ticks;
					os.write("Proxy added");
				}
				else {
					os.write($"Invalid proxy strength '{args[0]}' (must be valid float)");
				}
			}
			else {
				os.write("Proxy already present");
			}
		}
	}

	[Command("removeProxy", "delProxy")]
	[Description("Removes a proxy from the connected system")]
	[Arguments()]
	public static void RemoveProxy(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.hasProxy) {
				c.hasProxy = c.proxyActive = false;
				c.startingOverloadTicks = c.proxyOverloadTicks = 0;
				os.write("Proxy disabled");
			}
			else {
				os.write("No proxy present");
			}
		}
	}

	[Command("bypassProxy", "snap")]
	[Description("Instantly overloads the proxy for the connected system")]
	[Arguments()]
	public static void DisableProxy(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.hasProxy) {
				c.proxyOverloadTicks = 0;
				c.proxyActive = false;
				os.write("Proxy disabled");
			}
			else {
				os.write("No proxy present");
			}
		}
	}

	[Command("resetProxy", "unsnap", "blip")]
	[Description("Instantly resets the proxy the connected system")]
	[Arguments()]
	public static void EnableProxy(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.hasProxy) {
				c.proxyOverloadTicks = c.startingOverloadTicks;
				c.proxyActive = true;
				os.write("Proxy enabled");
			}
			else {
				os.write("No proxy present");
			}
		}
	}

	#endregion

	#region Ports

	[Command("openAllPorts", "unlock")]
	[Description("Instantly opens all ports, unconditionally")]
	[Arguments()]
	public static void OpenAllPorts(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			bool hadPorts = false;
			string source = os.thisComputer.ip;

			foreach (PortState port in c.GetAllPortStates()) {
				if (port.Cracked)
					continue;

				hadPorts = true;
				port.SetCracked(true, source);
				os.write($"Opened {port.DisplayName} port ({port.Record.Protocol}, {port.PortNumber})");
			}

			if (!hadPorts)
				os.write("No ports to open");
		}
	}

	[Command("closeAllPorts", "lock")]
	[Description("Instantly closes all ports")]
	[Arguments()]
	public static void CloseAllPorts(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			bool hadPorts = false;
			string source = os.thisComputer.ip;

			foreach (PortState port in c.GetAllPortStates()) {
				if (!port.Cracked)
					continue;

				hadPorts = true;
				port.SetCracked(false, source);
				os.write($"Closed {port.DisplayName} port ({port.Record.Protocol}, {port.PortNumber})");
			}

			if (!hadPorts)
				os.write("No ports to close");
		}
	}

	[Command("openPorts", "open")]
	[Description("Opens the listed port(s), if active")]
	[Arguments("ports...")]
	public static void OpenSpecificPorts(OS os, string cmd, string[] args) {
		if (args.Length < 1) {
			os.write($"Usage: {cmd} [<port> [<port2>...]]");
			return;
		}
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			string me = os.thisComputer.ip;

			HashSet<int> ports = new(args
				.SelectMany(s => s.Split(','))
				.Select(s => s.Trim())
				.Where(s => int.TryParse(s, out _))
				.Select(int.Parse)
				.ToArray()
			);

			foreach (PortState port in c.GetAllPortStates()) {
				if (ports.Contains(port.PortNumber) && !port.Cracked) {
					port.SetCracked(true, me);
					os.write($"Opened port {port.PortNumber} ({port.DisplayName})");
				}
			}
		}
	}

	[Command("closePorts", "close")]
	[Description("Closes the listed port(s), if active")]
	[Arguments("ports...")]
	public static void CloseSpecificPorts(OS os, string cmd, string[] args) {
		if (args.Length < 1) {
			os.write($"Usage: {cmd} [<port> [<port2>...]]");
			return;
		}
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			string me = os.thisComputer.ip;

			HashSet<int> ports = new(args
				.SelectMany(s => s.Split(','))
				.Select(s => s.Trim())
				.Where(s => int.TryParse(s, out _))
				.Select(int.Parse)
				.ToArray()
			);

			foreach (PortState port in c.GetAllPortStates()) {
				if (ports.Contains(port.PortNumber) && port.Cracked) {
					port.SetCracked(false, me);
					os.write($"Closed port {port.PortNumber} ({port.DisplayName})");
				}
			}
		}
	}

	// TODO: add/remove ports
	// TODO: change ports needed for crack

	#endregion

	#region Cracking

	[Command("resetSecurity", "reset")]
	[Description("Instantly closes all ports and resets firewall/proxy")]
	[Arguments("target?")]
	public static void ResetSecurity(OS os, string cmd, string[] args) {
		Computer c = args.Length > 0
			? Programs.getComputer(os, args[0])
			: os.connectedComp;

		if (c is not null) {
			bool hadPorts = false;
			string source = os.thisComputer.ip;

			foreach (PortState port in c.GetAllPortStates()) {
				if (!port.Cracked)
					continue;

				hadPorts = true;
				port.SetCracked(false, source);
				os.write($"Closed {port.DisplayName} port ({port.Record.Protocol}, {port.PortNumber})");
			}

			if (!hadPorts)
				os.write("No ports to close");

			if (c.firewall is not null) {
				c.firewall.solved = false;
				os.write($"Firewall locked ({c.firewall.solution ?? "<no solution?>"})");
			}
			else {
				os.write("No firewall present");
			}

			if (c.hasProxy) {
				c.proxyActive = true;
				os.write("Proxy enabled");
			}
			else {
				os.write("No proxy present");
			}

		}
		else {
			os.write("Target computer not found");
		}
	}

	[Command("bypass", "zoom", "skip")]
	[Description("Instantly hacks the connected computer, IF you have the portcrushers")]
	[Arguments()]
	public static void FastHack(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			List<PortState> ports = c.GetAllPortStates();

			if (c.GetRealPortsNeededForCrack() > ports.Count) {
				os.write("Cannot bypass: more ports are needed to crack than are available");
				return;
			}

			if (ports.Count > 0) {
				Folder bin = os.thisComputer.files.root.searchForFolder("bin");

				List<PortState> cannotOpen = new();
				List<PortState> canOpen = new();
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

			ForceHack(os, cmd, args);
		}
	}

	[Command("ForceHack")]
	[Description("Instantly hacks the connected computer, unconditionally")]
	[Arguments()]
	public static void ForceHack(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			SolveFirewall(os, cmd, args);
			DisableProxy(os, cmd, args);
			OpenAllPorts(os, cmd, args);

			c.userLoggedIn = true;
			os.write("Logged in");

			os.takeAdmin();
			os.write("Admin access granted");
		}
	}

	[Command("FastHack")]
	[Description("PortHack, but instant")]
	[Arguments()]
	public static void InstantPorthack(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.GetRealPortsNeededForCrack() > 0) {
				int opened = 0;

				foreach (PortState port in c.GetAllPortStates()) {
					if (port.Cracked)
						++opened;
				}

				if (opened < c.GetRealPortsNeededForCrack()) {
					os.write("Not enough ports open");
					return;
				}
			}

			if (c.hasProxy && c.proxyActive) {
				os.write("Proxy still active");
				return;
			}

			if (c.firewall is not null && !c.firewall.solved) {
				os.write("Firewall still active");
				return;
			}

			c.userLoggedIn = true;
			os.write("Logged in");

			os.takeAdmin();
			os.write("Admin access granted");
		}
	}

	#endregion

	#region Traces

	[Command("evade", "survive")]
	[Description("Kills an ongoing trace, including ETAS")]
	[Arguments()]
	public static void StopTrace(OS os, string cmd, string[] args) {
		if (os.traceTracker.active) {
			os.traceTracker.stop();
			os.write("Killed current trace");
		}
		if (os.TraceDangerSequence.IsActive) {
			os.TraceDangerSequence.CancelTraceDangerSequence();
			os.TraceDangerSequence.percentComplete = 0;
			os.write("ETAS terminated");
		}
	}

	[Command("tkill", "tk")]
	[Description("Instantly launches TraceKill.exe, IF you have enough ram")]
	[Arguments()]
	public static void InstantTraceKill(OS os, string cmd, string[] args) {
		const string name = "TraceKill.exe";
		string magic = Foxnet.GetMagicFromExeName(name);

		if (!string.IsNullOrEmpty(magic))
			os.launchExecutable(name, magic, 0);
		else
			os.write($"Foxnet error: cannot find internal data for {name}");
	}

	[Command("resetOwnIP", "scramble", "fuck")]
	[Description("Immediately resets your own IP")]
	[Arguments()]
	public static void ResetOwnIP(OS os, string cmd, string[] args) {
		os.thisComputer.ip = NetworkMap.generateRandomIP();
		os.thisComputerIPReset();
		if (!os.thisComputer.disabled)
			os.rebootThisComputer();
	}

	[Command("clearLogs", "forget")]
	[Description("Immediately clears logs on the connected node")]
	[Arguments()]
	public static void DeleteAllLogs(OS os, string cmd, string[] args) {
		os.connectedComp?.files?.root?.searchForFolder("log")?.files?.Clear();
		os.write(os.connectedComp is null ? "Not connected, no logs to delete" : "Wiped all logs");
	}

	[Command("cleanDisconnect", "cleanDc", "bye")]
	[Description("Disconnects and wipes all logs")]
	[Arguments()]
	public static void CleanDisconnect(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer remote = os.connectedComp;
			remote.files?.root?.searchForFolder("log")?.files?.Clear();
			Programs.disconnect(new[] { "disconnect" }, os);
			remote.files?.root?.searchForFolder("log")?.files?.Clear();
			os.write($"Wiped remote logs on {remote.ip}");
		}
	}

	// TODO: add/remove trace timer

	#endregion

	#region General

	[Command("setRam", "archive")]
	[Description("Sets your max ram to the given amount, or 2 billion")]
	[Arguments("amount?")]
	public static void MaximiseRam(OS os, string cmd, string[] args) {
		if (args.Length < 1 || !int.TryParse(args[0], out int amount))
			amount = int.MaxValue;
		os.totalRam = amount;
	}

	[Command("resetIP", "changeIP")]
	[Description("Immediately resets the connected machine's IP")]
	[Arguments("target?")]
	public static void ResetIP(OS os, string cmd, string[] args) {
		Computer c = args.Length > 0
			? Programs.getComputer(os, args[0])
			: os.connectedComp;
		if (c is null) {
			os.write("Target computer not found");
			return;
		}
		if (c.ip == os.thisComputer.ip) {
			os.write("To change your own IP, use the resetOwnIP command.");
			return;
		}
		bool isConnected = c.ip == os.connectedComp.ip;
		c.ip = NetworkMap.generateRandomIP();
		if (isConnected)
			os.connectedIP = c.ip;
		os.write($"Changed target machine's IP to {c.ip}");
	}

	[Command("getAdmin", "takeAdmin", "giveAdmin", "claim")]
	[Description("Takes admin access on a computer if logged in")]
	[Arguments()]
	public static void TakeAdminAccess(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			if (os.connectedComp.userLoggedIn) {
				os.takeAdmin();
				os.write("Admin access granted");
			}
			else {
				os.write("You aren't logged in (did you want 'bypass' instead?)");
			}
		}
	}

	[Command("loseAdmin")]
	[Description("Lose admin access on a computer you have it on")]
	[Arguments()]
	public static void LoseAdminAccess(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			if (os.connectedComp.ip == os.thisComputer.ip) {
				os.write("Cannot lose admin on your own computer");
			}
			else if (os.connectedComp.PlayerHasAdminPermissions()) {
				os.connectedComp.adminIP = os.connectedComp.ip;
				os.write("Admin access lost");
			}
			else {
				os.write("You aren't an admin");
			}
		}
	}

	[Command("authenticate", "auth", "whitelist")]
	[Description("Adds you to an authenticator IP list")]
	[Arguments("target?")]
	public static void AddToAuthenticator(OS os, string cmd, string[] args) {
		Computer c = args.Length > 0
			? Programs.getComputer(os, args[0])
			: os.connectedComp;
		if (c is not null) {
			string me = os.thisComputer.ip;
			Folder app = c.files.root.searchForFolder("Whitelist");
			FileEntry? sourceFile = app?.searchForFile("source.txt");
			FileEntry? localList = app?.searchForFile("list.txt");
			string? target = sourceFile?.data?.Trim();

			if (localList is not null) {
				localList.data = string.Join("\n", new HashSet<string>(
					  $"{localList.data.Trim()}\n{me}"
						.Split('\n')
						.Where(s => !string.IsNullOrWhiteSpace(s))
				));
				os.write($"Added to IP list");
				return;
			}

			if (!string.IsNullOrEmpty(target)) {
				Computer daemon = Programs.getComputer(os, target);
				Folder? remoteApp = daemon?.files?.root?.searchForFolder("Whitelist");
				FileEntry? remoteList = remoteApp?.searchForFile("list.txt");

				if (remoteList is not null) {
					remoteList.data = string.Join("\n", new HashSet<string>(
						  $"{remoteList.data.Trim()}\n{me}"
							.Split('\n')
							.Where(s => !string.IsNullOrWhiteSpace(s))
					));
					os.netMap.discoverNode(daemon);
					os.write($"Identified authenticator {target}, added to IP list");
					return;
				}
			}

			os.write("Couldn't find local or remote authenticator");
		}
	}

	[Command("examine")]
	[Description("Displays information about the connected or given computer")]
	[Arguments("target?")]
	public static void ExamineComputer(OS os, string cmd, string[] args) {
		const string none = "[n/a]";
		const double delay = 0.2;

		Computer c = args.Length > 0
			? Programs.getComputer(os, args[0])
			: os.connectedComp ?? os.thisComputer;

		if (c is not null) {
			Administrator admin = c.admin;
			string owner = c.adminIP;
			string pass = c.adminPass;
			Firewall firewall = c.firewall;
			List<PortState> ports = c.GetAllPortStates() ?? new();
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

			List<string> rawLines = new(new[] {
				$"State of {ip}/{id}: {name}",
				$"Current admin: {owner} ({pass})",
				$"Admin access: {(isAdmin ? "" : "not ")}available",
				$"Sysadmin: {adminType}",
				$"Security level: {security}",
				$"Proxy: {proxyDesc}",
				$"Firewall: {firewallDesc}",
				portDesc,
				crackDesc
			});

			if (ports.Count < needed) {
				rawLines.Add("This computer cannot be hacked!");
			}
			else if (ports.Count == 0) {
				rawLines.Add("This computer can always be hacked.");
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
					rawLines.Add($"You cannot currently hack this computer.");
					rawLines.Add($"You must open {(missing == 1 ? "1 port" : $"{missing} ports")} more than you have tools for.");
				}
			}

			if (traceTime > 0)
				rawLines.Add($"Trace time: {traceTime}s");
			else
				rawLines.Add("No trace present");

			if (tracked)
				rawLines.Add("Tracker present");
			else
				rawLines.Add("No tracker present");

			rawLines.Add($"System memory: {memDesc}");

			List<SlowPrinter.DelayedLine> lines = SlowPrinter.ConstructUniformDelay(delay, rawLines.ToArray());

			lines.Add(new(0.5, "\n"));

			string[] taglines = Foxnet.Snark.Split('\n');
			foreach (string tagline in taglines) {
				lines.Add(new(1, $"// {tagline}"));
			}

			lines.Add(new(0.5, "\n"));

			SlowPrinter.SlowPrint(os, lines.ToArray());
		}
		else {
			os.write("Target computer not found");
		}
	}

	[Command("killall", "ka")]
	[Description("Kills all running processes with the given name")]
	[Arguments("name")]
	public static void KillProcessesByFullName(OS os, string cmd, string[] args) {
		string target = args[0].ToLower();
		bool found = false;
		foreach (ExeModule exe in os.exes.ToArray()) {
			if ((exe.name ?? exe.IdentifierName ?? string.Empty).ToLower() == target) {
				exe.Kill();
				os.write($"Killed {exe.IdentifierName ?? exe.name} ({exe.PID})");
				found = true;
			}
		}
		if (!found)
			os.write("No matching processes found (did you mean to use killalls?)");
	}

	[Command("killalls", "ks")]
	[Description("Kills all running processes with the given name")]
	[Arguments("name")]
	public static void KillProcessesByPartialName(OS os, string cmd, string[] args) {
		string target = args[0].ToLower();
		bool found = false;
		foreach (ExeModule exe in os.exes.ToArray()) {
			if ((exe.name ?? exe.IdentifierName ?? string.Empty).ToLower().Contains(target)) {
				exe.Kill();
				os.write($"Killed {exe.IdentifierName ?? exe.name} ({exe.PID})");
				found = true;
			}
		}
		if (!found)
			os.write("No matching processes found");
	}

	[Command("hideLogs", "stealth", "sneak")]
	[Description("Controls whether your actions leave logs on the target computer")]
	[Arguments("mode?")]
	public static void ToggleStealthMode(OS os, string cmd, string[] args) {
		switch (args.Length == 0 ? string.Empty : args[0].ToLower()) {
			case "on":
				Hooks.BlockComputerLogs = true;
				break;
			case "off":
				Hooks.BlockComputerLogs = false;
				break;
			case "toggle":
				Hooks.BlockComputerLogs ^= true;
				break;
		}
		os.write($"Your logs are{(Hooks.BlockComputerLogs ? "" : " not")} being blocked");
	}

	[Command("foxlol")]
	[Description("Displays a silly/funny comment")]
	[Arguments()]
	public static void FunnyComment(OS os, string cmd, string[] args) {
		List<SlowPrinter.DelayedLine> lines = new();
		string[] taglines = Foxnet.Snark.Split('\n');

		lines.Add(new(0, $"// {taglines[0]}"));

		if (taglines.Length > 1) {
			foreach (string tagline in taglines.Skip(1))
				lines.Add(new(1, $"// {tagline}"));
		}

		lines.Add(new(0.5, "\n"));

		SlowPrinter.SlowPrint(os, lines.ToArray());
	}

	[Command("xmas")]
	[Description("Gives you a copy of EVERY executable in the game")]
	[Arguments()]
	public static void GetAllPrograms(OS os, string cmd, string[] args) {
		Folder bin = os.thisComputer.files.root.searchForFolder("bin");
		HashSet<string> progs = new(bin.files.Select(f => f.data));
		List<FileEntry> files = new();
		foreach (int exeNum in PortExploits.exeNums) {
			string exeName = PortExploits.cracks[exeNum];
			string magic = PortExploits.crackExeData[exeNum];

			if (!progs.Contains(magic))
				files.Add(new(magic, exeName));
		}
		// unfortunately, there's no way to list all registered custom executables
		// so we have to reflect into the manager instead
		Type exeManager = typeof(ExecutableManager);
		try {
			Type customExeInfoType = exeManager.GetNestedType("CustomExeInfo", BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo customExeList = exeManager.GetField("CustomExes", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			object listObj = customExeList.GetValue(null); // List<struct CustomExeInfo> but that struct is private, hence all the damn reflection
			Type list = listObj.GetType();
			MethodInfo get = (list.GetMethod("ElementAt") ?? typeof(IEnumerable<>).GetMethod("ElementAt") ?? typeof(Enumerable).GetMethod("ElementAt")
				?? throw new NullReferenceException("cannot reflect IEnumerable<>.ElementAt method")).MakeGenericMethod(customExeInfoType);
			int count = (int)list.GetProperty("Count").GetValue(listObj);
			Type ceType = null!;
			FieldInfo exeDataField = null!;
			FieldInfo exeTypeField = null!;
			for (int i = 0; i < count; ++i) {
				object ceStruct = get.Invoke(listObj, new object[] { listObj, i });
				if (ceStruct is null) {
					os.write($"Custom executable ${i + 1}/${count} is null");
					continue;
				}
				ceType ??= ceStruct.GetType();
				exeDataField ??= ceType.GetField("ExeData")
					?? throw new NullReferenceException("cannot reflect CustomExeInfo.ExeData field");
				exeTypeField ??= ceType.GetField("ExeType")
					?? throw new NullReferenceException("cannot reflect CustomExeInfo.ExeType field");
				string magic = (string)exeDataField.GetValue(ceStruct)
					?? throw new NullReferenceException("failed to reflect contents of CustomExeInfo.ExeData field");
				string name = ((Type)exeTypeField.GetValue(ceStruct) ?? throw new NullReferenceException("failed to reflect contents of CustomExeInfo.ExeType field")).FullName;
				if (!progs.Contains(magic))
					files.Add(new(magic, name + ".exe"));
			}
		}
		catch (Exception e) {
			os.write($"Failed to reflect into {exeManager.FullName}");
			os.write($"{e.GetType().Name}:\n{e.Message}");
			os.write("Cannot provide custom executables");
		}
		bin.files.AddRange(files);
		os.write($"Added {files.Count} program{(files.Count == 1 ? "" : "s")} to your /bin folder.");
		if (files.Count > 0)
			os.write("Ho ho ho, ya naughty fuck.");
	}

	#endregion

}
