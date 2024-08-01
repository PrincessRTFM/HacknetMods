using System.Collections.Generic;
using System.Linq;

using Hacknet;

using Pathfinder.Port;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ClosePorts: CommandBase {
	public override string Description { get; } = "Closes the listed port(s), if active";
	public override string[] Arguments { get; } = ["ports..."];

	public override void Execute(OS os, string cmd, string[] args) {
		if (args.Length < 1) {
			Foxnet.Libsune.Terminal.Print($"Usage: {cmd} [<port> [<port2>...]]");
			return;
		}
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;
			string me = os.thisComputer.ip;

			HashSet<int> ports = new(args
				.SelectMany(s => s.Split(','))
				.Select(s => s.Trim())
				.Where(s => int.TryParse(s, out _))
				.Select(int.Parse)
				.ToArray()
			);

			foreach (PortState port in c.GetAllPortStates()) {
				if (ports.Contains(port.PortNumber) && port.Cracked) {
					port.SetCracked(false, me);
					Foxnet.Libsune.Terminal.Print($"Closed port {port.PortNumber} ({port.DisplayName})");
				}
			}
		}
	}
}
