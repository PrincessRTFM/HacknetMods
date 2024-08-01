using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class RemoveProxy: CommandBase {
	public override string[] Aliases { get; } = ["DelProxy"];
	public override string Description { get; } = "Removes a proxy from the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.hasProxy) {
				c.hasProxy = c.proxyActive = false;
				c.startingOverloadTicks = c.proxyOverloadTicks = 0;
				Foxnet.Libsune.Terminal.Print("Proxy disabled");
			}
			else {
				Foxnet.Libsune.Terminal.Print("No proxy present");
			}
		}
	}
}
