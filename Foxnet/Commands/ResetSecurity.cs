using Hacknet;

using Pathfinder.Port;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ResetSecurity: CommandBase {
	public override string Description { get; } = "Instantly closes all ports and resets firewall/proxy";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
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

			Foxnet.PrintRandomSnark(os);
		}
		else {
			os.write("Target computer not found");
		}
	}
}
