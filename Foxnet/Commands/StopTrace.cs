using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class StopTrace: CommandBase {
	public override string[] Aliases { get; } = ["evade", "survive"];
	public override string Description { get; } = "Kills an ongoing trace, including ETAS";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		bool worked = false;
		if (os.traceTracker.active) {
			os.traceTracker.stop();
			Foxnet.Libsune.Terminal.Print("Killed current trace");
			worked = true;
		}
		if (os.TraceDangerSequence.IsActive) {
			os.TraceDangerSequence.CancelTraceDangerSequence();
			os.TraceDangerSequence.percentComplete = 0;
			Foxnet.Libsune.Terminal.Print("ETAS terminated");
			worked = true;
		}
		if (os.TrackersInProgress.Count > 0) {
			int trackers = os.TrackersInProgress.Count;
			os.TrackersInProgress.Clear();
			Foxnet.Libsune.Terminal.Print($"Killed {trackers} tracker{(trackers == 1 ? "" : "s")}");
			worked = true;
		}

		if (worked)
			Foxnet.PrintRandomSnark(os);
	}
}
