namespace PrincessRTFM.Hacknet.Foxnet;

internal struct CommandData {
	public string PrettyName;
	public string Description;
	public ArgumentsAttribute Arguments;
	public Foxnet.PluginCommandDelegate Handler;

	public readonly string Usage => $"{this.PrettyName} {this.Arguments?.ArgumentDescription ?? ""}".Trim();
}
