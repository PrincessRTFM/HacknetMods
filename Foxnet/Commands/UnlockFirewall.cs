using System;

using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class UnlockFirewall: CommandBase {
	public override string[] Aliases { get; } = ["SolveFirewall", "freeze"];
	public override string Description { get; } = "Instantly solves the firewall on the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
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
}
