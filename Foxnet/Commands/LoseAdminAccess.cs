using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class LoseAdminAccess: CommandBase {
	public override string Description { get; } = "Lose admin access on a computer you have it on";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			if (os.connectedComp.ip == os.thisComputer.ip) {
				Foxnet.Libsune.Terminal.Print("Cannot lose admin on your own computer");
			}
			else if (os.connectedComp.PlayerHasAdminPermissions()) {
				os.connectedComp.adminIP = os.connectedComp.ip;
				Foxnet.Libsune.Terminal.Print("Admin access lost");
			}
			else {
				Foxnet.Libsune.Terminal.Print("You aren't an admin");
			}
		}
	}
}
