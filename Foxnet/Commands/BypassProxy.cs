using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class BypassProxy: CommandBase {
	public override string[] Aliases { get; } = ["snap"];
	public override string Description { get; } = "Instantly overloads the proxy for the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.hasProxy) {
				c.proxyOverloadTicks = 0;
				c.proxyActive = false;
				os.write("Proxy disabled");
				Foxnet.PrintRandomSnark(os);
			}
			else {
				os.write("No proxy present");
			}
		}
	}
}
