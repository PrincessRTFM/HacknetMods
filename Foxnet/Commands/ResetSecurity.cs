using Hacknet;

using Pathfinder.Port;

using PrincessRTFM.Hacknet.Lib;

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
				os.Print($"Closed {port.DisplayName} port ({port.Record.Protocol}, {port.PortNumber})");
			}

			if (!hadPorts)
				os.Print("No ports to close");

			if (c.firewall is not null) {
				c.firewall.solved = false;
				os.Print($"Firewall locked ({c.firewall.solution ?? "<no solution?>"})");
			}
			else {
				os.Print("No firewall present");
			}

			if (c.hasProxy) {
				c.proxyActive = true;
				os.Print("Proxy enabled");
			}
			else {
				os.Print("No proxy present");
			}

			Foxnet.PrintRandomSnark(os);
		}
		else {
			os.Print("Target computer not found");
		}
	}
}
