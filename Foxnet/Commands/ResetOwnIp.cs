using System.Linq;

using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class ResetOwnIp: CommandBase {
	public override string Description { get; } = "Immediately resets your own IP; reboots unless used with -s";
	public override string[] Arguments { get; } = ["-s?"];

	public override void Execute(OS os, string cmd, string[] args) {
		os.thisComputer.ip = NetworkMap.generateRandomIP();
		os.thisComputerIPReset();
		if (!os.thisComputer.disabled && !args.Any(s => s == "-s"))
			os.rebootThisComputer();
	}
}
