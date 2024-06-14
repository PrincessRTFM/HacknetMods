using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Hacknet;

using HarmonyLib;

using Pathfinder.Port;
using Pathfinder.Util;

namespace PrincessRTFM.Hacknet.Foxnet;

[HarmonyPatch]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "parameter names must conform to Harmony specs in order to work")]
public static class Hooks {
	private static FieldInfo getField(Type type, string name) => AccessTools.Field(type, name) ?? throw new NullReferenceException($"failed to reflect field {type.FullName}.{name}");
	private static MethodInfo getMethod(Type type, string name) => AccessTools.Method(type, name) ?? throw new NullReferenceException($"failed to reflect method {type.FullName}.{name}(...)");
	private static MethodInfo getMethod(Type type, string name, params Type[] args) => AccessTools.Method(type, name, args)
		?? throw new NullReferenceException($"failed to reflect method {type.FullName}.{name}{args.Description()}");

	#region Stealth mode (blocking logs)
	// lang=regex
	private const string UserLogPrefix = @"\w+:?[\s_]+(?:by|from)[\s_]+";
	private static readonly Dictionary<string, Regex> logFilters = [];

	public static bool BlockComputerLogs { get; set; }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Computer), nameof(Computer.log))]
	private static bool CheckIfLoggingAllowed(Computer __instance, string message) {
		if (!BlockComputerLogs) {
			Foxnet.Info($"Not filtering log to {__instance.ip}: {message}");
			return true;
		}
		string ip = OS.currentInstance.thisComputer.ip;
		if (!logFilters.TryGetValue(ip, out Regex? filter)) {
			filter = new(UserLogPrefix + Regex.Escape(ip), RegexOptions.IgnoreCase | RegexOptions.Compiled);
			logFilters[ip] = filter;
		}
		bool loggingUserAction = filter.IsMatch(message) || message.StartsWith(ip);
		if (loggingUserAction) {
			Foxnet.Info($"Blocking user-action log for {__instance.ip}: {message}");
			return false;
		}
		Foxnet.Info($"Allowing filtered log for {__instance.ip}: {message}");
		return true;
	}

	#endregion

	#region Portcrusher target port auto-selection
	private static readonly Dictionary<string, string> sslTunnelPorts = new() {
		{ "ssh", "-s" },
		{ "ftp", "-f" },
		{ "web", "-w" },
		{ "rtsp", "-r" },
	};

	public static bool AutoSelectPorts { get; set; }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProgramRunner), nameof(ProgramRunner.AttemptExeProgramExecution))]
	private static void InjectPortArguments(OS os, ref string[] p) {
		if (os.connectedComp is not Computer remote) // if not connected to a target, abort
			return;
		string name = p[0];
		string[] args = p.Skip(1).ToArray();
		if (args.Length > 0) // if any arguments were passed, abort
			return;
		Computer local = os.thisComputer;
		Folder bin = local.files.root.searchForFolder("bin");
		int fileIndexOfExeProgram = ProgramRunner.GetFileIndexOfExeProgram(name, bin);
		if (fileIndexOfExeProgram == int.MaxValue || fileIndexOfExeProgram < 0 || fileIndexOfExeProgram > bin.files.Count) // if the thing being run isn't a "real" program, abort
			return;
		string magic = bin.files[fileIndexOfExeProgram].data;
		int progId = -1;
		foreach (int exeNum in PortExploits.exeNums) {
			if (PortExploits.crackExeData[exeNum] == magic || PortExploits.crackExeDataLocalRNG[exeNum] == magic) {
				progId = exeNum;
				break;
			}
		}
		if (progId < 0) // if there's no program ID, abort (because we can't find the target port from custom stuff)
			return;
		if (progId == 211) // FTPSprint -> FTPBounce
			progId = 21;
		if (!PortExploits.needsPort[progId]) // if the program doesn't use a target port, abort (nothing to do)
			return;

		PortRecord port = PortManager.GetPortRecordFromNumber(progId);
		if (string.IsNullOrEmpty(port?.Protocol)) // if we can't find the port's protocol name, abort
			return;

		PortState state = remote.GetPortState(port!.Protocol);
		if (state is null) // if the computer doesn't have that port on it, abort
			return;

		int target = state.PortNumber;
		if (progId == 443) { // SSLTrojan requires a port opened for use
			foreach (KeyValuePair<string, string> protoFlag in sslTunnelPorts) {
				PortState candidate = remote.GetPortState(protoFlag.Key);
				if (candidate?.Cracked is not true)
					continue;
				p = [
					name,
					target.ToString(),
					protoFlag.Value,
					candidate.PortNumber.ToString(),
				];
				break;
			}
		}
		else {
			p = [name, target.ToString()];
		}

		if (p.Length > 1) {
			os.terminal.lastRunCommand = string.Join(" ", p);
			os.display.command = name;
			os.display.commandArgs = p;
			os.display.typeChanged();
			os.write($"Target port auto-selected by Foxnet! ouo >> {os.terminal.lastRunCommand}");
		}
	}

	#endregion

	#region Allow `connect`ing to nodes by ID

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Programs), nameof(Programs.connect))]
	private static void ConnectToComputerById(ref string[] args, OS os) {
		if (args.Length < 2)
			return;
		Computer found = Programs.getComputer(os, args[1]);
		if (found is null)
			return;
		args[1] = found.ip;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Programs), nameof(Programs.computerExists))]
	private static void CheckComputerExistenceById(string ip, ref bool __result) => __result = __result || (ComputerLookup.FindById(ip) is not null);

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Programs), nameof(Programs.scan))]
	private static void ScanComputerById(string[] args, OS os) {
		if (args.Length < 2)
			return;

		Computer computer = ComputerLookup.FindById(args[1]);
		if (computer is null)
			return;

		os.netMap.discoverNode(computer);
		os.write("Found Terminal : " + computer.name + "@" + computer.ip);
	}

	#endregion

}
