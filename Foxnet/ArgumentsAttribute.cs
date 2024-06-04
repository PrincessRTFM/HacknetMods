using System;
using System.Linq;

namespace PrincessRTFM.Hacknet.Foxnet;

[AttributeUsage(AttributeTargets.Method)]
internal class ArgumentsAttribute: Attribute {
	public string ArgumentDescription
		=> string.Join(" ", this.Arguments.Select(a => a.EndsWith("?") ? $"[{a.TrimEnd('?')}]" : $"<{a}>"));
	public string[] Arguments { get; }
	public int RequiredArguments => this.Arguments.Where(a => !a.EndsWith("?")).Count();
	public int MaxArguments => this.Arguments.Length > 0 && this.Arguments.Last().EndsWith("...")
		? int.MaxValue
		: this.Arguments.Length;

	public ArgumentsAttribute(params string[] args) => this.Arguments = args;
}
