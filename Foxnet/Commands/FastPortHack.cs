using Hacknet;

using Pathfinder.Port;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class FastPortHack: CommandBase {
	public override string Description { get; } = "PortHack, but instant";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
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

			Foxnet.PrintRandomSnark(os);
		}
	}
}
