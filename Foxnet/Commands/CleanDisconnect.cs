using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class CleanDisconnect: CommandBase {
	public override string[] Aliases { get; } = ["CleanDc", "bye"];
	public override string Description { get; } = "Disconnects and wipes all logs";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer remote = os.connectedComp;
			Programs.disconnect(["disconnect"], os);
			remote.files?.root?.searchForFolder("log")?.files?.Clear();
			os.write($"Wiped remote logs on {remote.ip}");
			Foxnet.PrintRandomSnark(os);
		}
	}
}
