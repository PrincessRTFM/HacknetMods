using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class RemoveFirewall: CommandBase {
	public override string[] Aliases { get; } = ["DelFirewall"];
	public override string Description { get; } = "Removes a firewall from the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is not null) {
				c.firewall = null;
				Foxnet.Libsune.Terminal.Print($"Firewall removed");
			}
			else {
				Foxnet.Libsune.Terminal.Print("No firewall present");
			}
		}
	}
}
