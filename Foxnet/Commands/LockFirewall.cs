using Hacknet;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class LockFirewall: CommandBase {
	public override string[] Aliases { get; } = ["ResetFirewall", "thaw"];
	public override string Description { get; } = "Resets the firewall on the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				c.firewall.solved = false;
				c.firewall.resetSolutionProgress();
				os.Print($"Firewall locked ({c.firewall.solution ?? "<no solution?>"})");
			}
			else {
				os.Print("No firewall present");
			}
		}
	}
}
