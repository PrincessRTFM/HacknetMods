using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet;

public static class Extensions {

	public static int GetRealPortsNeededForCrack(this Computer c) => c.portsNeededForCrack + 1;
	public static void SetRealPortsNeededForCrack(this Computer c, int ports) => c.portsNeededForCrack = ports - 1;

}
