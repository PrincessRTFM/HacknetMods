using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class LaunchTraceKill: CommandBase {
	public override string[] Aliases { get; } = ["tkill", "tk"];
	public override string Description { get; } = "Instantly launches TraceKill.exe, IF you have enough ram";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		const string name = "TraceKill.exe";
		string magic = Foxnet.GetMagicFromExeName(name);

		if (!string.IsNullOrEmpty(magic))
			os.launchExecutable(name, magic, 12); // target port isn't even used in the method, wtf
		else
			os.write($"Foxnet error: cannot find internal data for {name}");
	}
}
