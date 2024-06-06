using System;

using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class BypassProxy: CommandBase {
	public override string[] Aliases { get; } = ["snap"];
	public override string Description { get; } = "Instantly overloads the proxy for the connected system";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) => throw new NotImplementedException();
}
