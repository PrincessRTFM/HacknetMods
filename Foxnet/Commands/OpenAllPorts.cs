using Hacknet;

using Pathfinder.Port;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class OpenAllPorts: CommandBase {
	public override string[] Aliases { get; } = ["unlock"];
	public override string Description { get; } = "Instantly opens all ports, unconditionally";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			bool hadPorts = false;
			string source = os.thisComputer.ip;

			foreach (PortState port in c.GetAllPortStates()) {
				if (port.Cracked)
					continue;

				hadPorts = true;
				port.SetCracked(true, source);
				Foxnet.Libsune.Terminal.Print($"Opened {port.DisplayName} port ({port.Record.Protocol}, {port.PortNumber})");
			}

			if (!hadPorts)
				Foxnet.Libsune.Terminal.Print("No ports to open");
		}
	}
}
