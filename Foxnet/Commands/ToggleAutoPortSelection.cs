using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ToggleAutoPortSelection: CommandBase {
	public override string[] Aliases { get; } = ["smart"];
	public override string Description { get; } = "Controls whether vanilla portcrushers auto-select their target port arguments";
	public override string[] Arguments { get; } = ["on/off/toggle?"];

	public override void Execute(OS os, string cmd, string[] args) {
		switch (args.Length == 0 ? string.Empty : args[0].ToLower()) {
			case "on":
				Hooks.AutoSelectPorts = true;
				break;
			case "off":
				Hooks.AutoSelectPorts = false;
				break;
			case "toggle":
				Hooks.AutoSelectPorts ^= true;
				break;
		}
		Foxnet.Libsune.Terminal.Print($"Vanilla portcrushers are {(Hooks.AutoSelectPorts ? "smart" : "dumb")}");
		Foxnet.PrintRandomSnark(os);
	}
}
