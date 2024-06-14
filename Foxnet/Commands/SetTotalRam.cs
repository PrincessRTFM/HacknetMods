using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class SetTotalRam: CommandBase {
	public const int DEFAULT_RAM = 761;
	public override string Description { get; } = $"Sets your max RAM to the given amount, defaulting to {DEFAULT_RAM}";
	public override string[] Arguments { get; } = ["amount?"];

	public override void Execute(OS os, string cmd, string[] args) {
		if (args.Length < 1 || !int.TryParse(args[0], out int amount))
			amount = DEFAULT_RAM;
		if (amount < 0)
			amount = int.MaxValue;
		int diff = amount - os.totalRam;
		if (diff > 0) { // increasing max ram - no checks needed
			os.totalRam += diff;
			os.ramAvaliable += diff;
		}
		else if (diff < 0) { // decreasing max ram - check that there'll be enough left
			int wouldBeLeft = os.ramAvaliable + diff;
			if (wouldBeLeft < 0) {
				os.write($"Cannot set max RAM to {amount}mb: too much is in use");
				os.write($"You must free at least {-wouldBeLeft}mb of RAM first");
			}
			else {
				os.totalRam += diff;
				os.ramAvaliable += diff;
				Foxnet.PrintRandomSnark(os);
			}
		}
		else {
			os.write($"No change - max RAM is already {amount}mb");
		}
		// update module bounds for the visuals
		os.ram.bounds.Height = os.totalRam + RamModule.contentStartOffset;
	}
}
