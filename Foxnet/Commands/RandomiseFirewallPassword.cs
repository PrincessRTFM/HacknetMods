using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class RandomiseFirewallPassword: CommandBase {
	public override string Command { get; } = "ResetFirewallPassword";
	public override string[] Aliases { get; } = ["randpass"];
	public override string Description { get; } = "Randomises the password for the firewall on the connected system";
	public override string[] Arguments { get; } = ["length?"];

	public override void Execute(OS os, string cmd, string[] args) {

		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				if (args.Length < 1 || !int.TryParse(args[0], out int len))
					len = 0;
				c.firewall.solutionLength = len;
				c.firewall.generateRandomSolution();
				os.write($"Firewall password changed to {c.firewall.solution ?? "<no solution?>"}");
			}
			else {
				os.write("No firewall present");
			}
		}
	}
}
