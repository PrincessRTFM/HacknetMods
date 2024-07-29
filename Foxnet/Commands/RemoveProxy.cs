using Hacknet;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class RemoveProxy: CommandBase {
	public override string Description { get; } = "Removes a proxy from the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.hasProxy) {
				c.hasProxy = c.proxyActive = false;
				c.startingOverloadTicks = c.proxyOverloadTicks = 0;
				os.Print("Proxy disabled");
			}
			else {
				os.Print("No proxy present");
			}
		}
	}
}
