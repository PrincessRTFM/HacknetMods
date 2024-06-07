using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class StopTrace: CommandBase {
	public override string[] Aliases { get; } = ["evade", "survive"];
	public override string Description { get; } = "Kills an ongoing trace, including ETAS";
	public override string[] Arguments { get; } = [];

	public override void Execute(OS os, string cmd, string[] args) {
		if (os.traceTracker.active) {
			os.traceTracker.stop();
			os.write("Killed current trace");
		}
		if (os.TraceDangerSequence.IsActive) {
			os.TraceDangerSequence.CancelTraceDangerSequence();
			os.TraceDangerSequence.percentComplete = 0;
			os.write("ETAS terminated");
		}
		if (os.TrackersInProgress.Count > 0) {
			int trackers = os.TrackersInProgress.Count;
			os.TrackersInProgress.Clear();
			os.write($"Killed {trackers} tracker{(trackers == 1 ? "" : "s")}");
		}
	}
}
