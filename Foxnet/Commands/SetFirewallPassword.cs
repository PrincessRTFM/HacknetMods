using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class SetFirewallPassword: CommandBase {
	public override string[] Aliases { get; } = ["setpass", "chpasswd"];
	public override string Description { get; } = "Changes the password for the firewall on the connected system";
	public override string[] Arguments { get; } = ["password"];

	public override void Execute(OS os, string cmd, string[] args) {

		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				string pass = args[0];
				if (pass.Length > Firewall.MIN_SOLUTION_LENGTH) {
					c.firewall.solution = args[0];
					c.firewall.solutionLength = args[0].Length;
					Foxnet.Libsune.Terminal.Print($"Firewall password changed to {c.firewall.solution ?? "<no solution?>"}");
				}
				else {
					Foxnet.Libsune.Terminal.Print($"Firewall password must be more than {Firewall.MIN_SOLUTION_LENGTH} characters");
				}
			}
			else {
				Foxnet.Libsune.Terminal.Print("No firewall present");
			}
		}
	}
}
