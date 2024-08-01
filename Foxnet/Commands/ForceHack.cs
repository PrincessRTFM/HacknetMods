using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ForceHack: CommandBase {
	public override string Description { get; } = "Instantly hacks the connected computer, unconditionally";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			Foxnet.RegisteredCommands[nameof(UnlockFirewall)].Execute(os, cmd, args);
			Foxnet.RegisteredCommands[nameof(BypassProxy)].Execute(os, cmd, args);
			Foxnet.RegisteredCommands[nameof(OpenAllPorts)].Execute(os, cmd, args);

			c.userLoggedIn = true;
			Foxnet.Libsune.Terminal.Print("Logged in");

			os.takeAdmin();
			Foxnet.Libsune.Terminal.Print("Admin access granted");

			Foxnet.PrintRandomSnark(os);
		}
	}
}
