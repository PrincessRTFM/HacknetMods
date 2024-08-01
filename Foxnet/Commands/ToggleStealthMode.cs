using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ToggleStealthMode: CommandBase {
	public override string[] Aliases { get; } = ["ToggleLogs", "stealth", "ghost"];
	public override string Description { get; } = "Controls whether your actions leave logs on the target computer";
	public override string[] Arguments { get; } = ["on/off/toggle?"];

	public override void Execute(OS os, string cmd, string[] args) {
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
		Foxnet.Libsune.Terminal.Print($"Your logs are{(Hooks.BlockComputerLogs ? "" : " not")} being blocked");
		Foxnet.PrintRandomSnark(os);
	}
}
