using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;

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