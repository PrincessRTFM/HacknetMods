using Hacknet;

using HarmonyLib;

namespace PrincessRTFM.Hacknet.Foxnet;

[HarmonyPatch]
public static class Hooks {
	public static bool BlockComputerLogs { get; set; }

	[HarmonyPatch(typeof(Computer), "log")]
	[HarmonyPrefix]
	public static bool OnLog() => !BlockComputerLogs;
}
