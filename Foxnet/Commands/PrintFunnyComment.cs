using System.Collections.Generic;
using System.Linq;

using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class PrintFunnyComment: CommandBase {
	public override string Command { get; } = "foxlol";
	public override string Description { get; } = "Displays a silly/funny comment";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) => Foxnet.PrintRandomSnark(os, false);
}
