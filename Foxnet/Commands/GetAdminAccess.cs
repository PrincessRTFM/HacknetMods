using Hacknet;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class GetAdminAccess: CommandBase {
	public override string[] Aliases { get; } = ["GetAdmin", "TakeAdminAccess", "TakeAdmin", "GiveAdminAccess", "GiveAdmin", "claim"];
	public override string Description { get; } = "Takes admin access on a computer, if you're logged in";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			if (os.connectedComp.userLoggedIn) {
				os.takeAdmin();
				os.Print("Admin access granted");
			}
			else {
				os.Print("You aren't logged in (did you want 'bypass' instead?)");
			}
		}
	}
}
