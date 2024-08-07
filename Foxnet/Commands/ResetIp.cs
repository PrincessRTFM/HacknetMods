using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ResetIp: CommandBase {
	public override string Description { get; } = "Immediately resets the target machine's IP, defaulting to the connected system";
	public override string[] Arguments { get; } = ["target?"];

	public override void Execute(OS os, string cmd, string[] args) {
		Computer c = args.Length > 0
			? Programs.getComputer(os, args[0])
			: os.connectedComp;
		if (c is null) {
			Foxnet.Libsune.Terminal.Print("Target computer not found");
			return;
		}
		if (c.ip == os.thisComputer.ip) {
			Foxnet.Libsune.Terminal.Print($"To change your own IP, use the {nameof(ResetOwnIp)} command.");
			return;
		}
		bool isConnected = c.ip == os.connectedComp.ip;
		c.ip = NetworkMap.generateRandomIP();
		if (isConnected)
			os.connectedIP = c.ip;
		Foxnet.Libsune.Terminal.Print($"Changed target machine's IP to {c.ip}");
		Foxnet.PrintRandomSnark(os);
	}
}
