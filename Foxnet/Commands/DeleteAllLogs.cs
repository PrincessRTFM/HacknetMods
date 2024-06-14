using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class DeleteAllLogs: CommandBase {
	public override string[] Aliases { get; } = ["ClearLogs", "forget"];
	public override string Description { get; } = "Immediately clears logs on the connected node";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not Computer c) {
			os.write("Not connected, no logs to delete");
			return;
		}
		c.files?.root?.searchForFolder("log")?.files?.Clear();
		os.write("Wiped all logs");
		Foxnet.PrintRandomSnark(os);
	}
}
