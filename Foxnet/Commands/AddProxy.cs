using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class AddProxy: CommandBase {
	public override string Description { get; } = "Adds a proxy to the connected system";
	public override string[] Arguments { get; } = ["strength"];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (!c.hasProxy) {
				if (float.TryParse(args[0], out float ticks)) {
					c.hasProxy = c.proxyActive = true;
					c.startingOverloadTicks = c.proxyOverloadTicks = ticks;
					os.write("Proxy added");
				}
				else {
					os.write($"Invalid proxy strength '{args[0]}' (must be valid float)");
				}
			}
			else {
				os.write("Proxy already present");
			}
		}
	}
}
