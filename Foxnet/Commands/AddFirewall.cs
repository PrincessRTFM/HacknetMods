using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class AddFirewall: CommandBase {
	public override string Description { get; } = "Adds a firewall to the connected system";
	public override string[] Arguments { get; } = ["complexity?", "solution?", "extra delay?"];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.connectedComp is not null) {
			Computer c = os.connectedComp;

			if (c.firewall is null) {
				int level = 0;
				string pass = "";
				float extra = 0;
				if (args.Length >= 1)
					int.TryParse(args[0], out level);
				if (args.Length >= 2)
					pass = args[1].Trim();
				if (args.Length >= 3)
					float.TryParse(args[0], out extra);
				Firewall f = new(level);
				if (!string.IsNullOrEmpty(pass)) {
					f.solution = pass;
					f.solutionLength = pass.Length;
				}
				f.additionalDelay = extra;
				c.firewall = f;
				os.write($"Firewall added ({c.firewall.solution ?? "<no solution?>"})");
			}
			else {
				os.write("Firewall already present");
			}
		}
	}
}
