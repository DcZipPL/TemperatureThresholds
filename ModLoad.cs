using HarmonyLib;
using PeterHan.PLib.Core;

namespace TemperatureThresholds
{
	public class ModLoad : KMod.UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			
			PUtil.InitLibrary(false);
		}
	}
}