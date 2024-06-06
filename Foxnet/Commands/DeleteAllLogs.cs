using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class DeleteAllLogs: CommandBase {
	public override string[] Aliases { get; } = ["ClearLogs", "forget"];
	public override string Description { get; } = "Immediately clears logs on the connected node";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		os.connectedComp?.files?.root?.searchForFolder("log")?.files?.Clear();
		os.write(os.connectedComp is null ? "Not connected, no logs to delete" : "Wiped all logs");
	}
}
