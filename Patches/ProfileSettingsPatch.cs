using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;

namespace TemperatureThresholds.Patches
{
	// ReSharper disable UnusedType.Global
	public class ProfileSettingsPatch
	{
		// Settings Screen
		[HarmonyPatch(typeof(UnitConfigurationScreen))]
		[HarmonyPatch("Init")]
		public static class AddProfilesButton
		{
			static AccessTools.FieldRef<UnitConfigurationScreen, GameObject> toggleGroupRef =
				AccessTools.FieldRefAccess<UnitConfigurationScreen, GameObject>("toggleGroup");

			public static void Postfix(UnitConfigurationScreen __instance)
			{
				PUtil.LogDebug("Injecting button into settings.");
				var btn = new PButton
				{
					Text = "Open Overlay Profiles",
					OnClick = sender =>
					{
						PUtil.LogDebug("Tried to open mod directory!");
						Process.Start(ModSettings.GetConfigPath());
					},
					ToolTip = "Opens TemperatureThresholds config directory"
				};
				btn.SetKleiBlueStyle();
				GameObject go = btn.AddTo(toggleGroupRef(__instance));
				go.SetActive(true);
			}
		}
	}
}