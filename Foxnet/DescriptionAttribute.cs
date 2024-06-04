using System;

namespace PrincessRTFM.Hacknet.Foxnet;

[AttributeUsage(AttributeTargets.Method)]
internal class DescriptionAttribute: Attribute {
	public string Description { get; }

	public DescriptionAttribute(string message) => this.Description = message;
}
