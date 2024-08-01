using BepInEx.Hacknet;

namespace PrincessRTFM.Hacknet.Lib;
public class LibsuneHN {
	internal const string LogTag = "[Libsune]";

	public TermLib Terminal { get; }

	public LibsuneHN(HacknetPlugin plugin) {
		plugin.Log.LogInfo($"{LogTag} Initialising LibsuneHN for {plugin.GetType().FullName}");
		this.Terminal = new(plugin);
	}

	#region IDisposable
	/*private bool disposed;

	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			// dispose managed state (managed objects)
		}
	}

	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
	*/
	#endregion
}
