using Hacknet;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ResetProxy: CommandBase {
	public override string[] Aliases { get; } = ["unsnap", "blip"];
	public override string Description { get; } = "Instantly resets the proxy the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.hasProxy) {
				c.proxyOverloadTicks = c.startingOverloadTicks;
				c.proxyActive = true;
				os.Print("Proxy enabled");
				Foxnet.PrintRandomSnark(os);
			}
			else {
				os.Print("No proxy present");
			}
		}
	}
}
