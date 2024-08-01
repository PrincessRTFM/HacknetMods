using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class GetAdminAccess: CommandBase {
	public override string[] Aliases { get; } = ["TakeAdminAccess", "GiveAdminAccess", "claim"];
	public override string Description { get; } = "Takes admin access on a computer, if you're logged in";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			if (os.connectedComp.userLoggedIn) {
				os.takeAdmin();
				Foxnet.Libsune.Terminal.Print("Admin access granted");
			}
			else {
				Foxnet.Libsune.Terminal.Print("You aren't logged in (did you want 'bypass' instead?)");
			}
		}
	}
}
