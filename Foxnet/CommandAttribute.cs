using System;

namespace PrincessRTFM.Hacknet.Foxnet;

[AttributeUsage(AttributeTargets.Method)]
internal class CommandAttribute: Attribute {
	public string Command { get; }
	public string[] Aliases { get; }

	public CommandAttribute(string cmd, params string[] aliases) {
		this.Command = cmd;
		this.Aliases = aliases;
	}
}
