using System;
using System.Linq;

using Hacknet;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet;

internal abstract class CommandBase {
	private string? cachedTypeName, cachedUsage;

	public virtual string Command {
		get {
			this.cachedTypeName ??= this.GetType().Name;
			return this.cachedTypeName;
		}
	}
	public virtual string[] Aliases { get; } = [];
	public abstract string Description { get; }
	public abstract string[] Arguments { get; }

	public string ArgumentDescription => string.Join(" ", this.Arguments.Select(a => a.EndsWith("?") ? $"[{a.TrimEnd('?')}]" : a));
	public int RequiredArguments => this.Arguments.Where(a => !a.EndsWith("?")).Count();
	public int MaxArguments => this.Arguments.Length > 0 && this.Arguments.Last().EndsWith("...")
		? int.MaxValue
		: this.Arguments.Length;

	public string Usage {
		get {
			this.cachedUsage ??= $"{this.Command} {this.ArgumentDescription}".Trim();
			return this.cachedUsage;
		}
	}

	public abstract void Execute(OS os, string cmd, string[] args);

	public void RedirectHacknetInvocation(OS os, string[] argv) {
		string arg0 = argv[0];
		string[] args = argv.Skip(1).ToArray();

		if (args.Length > this.MaxArguments || args.Length < this.RequiredArguments) {
			os.Print("Invalid usage (argument count mismatch)");
			os.Print($"Usage: {this.Command} {this.ArgumentDescription}".Trim());
			return;
		}

		try {
			this.Execute(os, arg0, args);
		}
		catch (Exception ex) {
			os.Print($"Foxnet command error in {this.GetType().Name}:\n> {ex.GetType().Name}\n>> {ex.Message}");
			os.Print(ex.StackTrace);
		}
	}
}
