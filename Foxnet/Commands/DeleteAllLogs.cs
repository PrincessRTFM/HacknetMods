using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class DeleteAllLogs: CommandBase {
	public override string[] Aliases { get; } = ["ClearLogs", "forget"];
	public override string Description { get; } = "Immediately clears logs on the connected node";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not Computer c) {
			Foxnet.Libsune.Terminal.Print("Not connected, no logs to delete");
			return;
		}
		c.files?.root?.searchForFolder("log")?.files?.Clear();
		Foxnet.Libsune.Terminal.Print("Wiped all logs");
		Foxnet.PrintRandomSnark(os);
	}
}
