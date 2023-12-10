using System.IO;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;

namespace TemperatureThresholds.Patches
{
	// ReSharper disable UnusedType.Global
	public class MigrationDialogPatch
	{
		[HarmonyPatch(typeof(MainMenu))]
		[HarmonyPatch("OnSpawn")]
		public class ShowPopup
		{
			public static void Postfix()
			{
				if (File.Exists(Path.Combine(ModSettings.GetConfigPath(), "config_migration")))
					return;
			    
				var label = new PLabel("TemperatureThresholdsLabel")
				{
					Text = "Temperature Thresholds config location has been moved from:\n\n" +
					       "<color=#FF0000>Documents\\Klei\\OxygenNotIncluded\\mods\\Steam\\{ModID}\\config</color>\n\n" +
					       "to:\n\n" +
					       "<color=#00FF00>Documents\\Klei\\OxygenNotIncluded\\mods\\config</color>\n\n" +
					       "Please move your config files to the new location and restart the game."
				};

				var dialog = new PDialog("TemperatureThresholdsDialog")
				{
					Title = "Config Location Changed",
				};
				
				dialog.DialogClosed += _ =>
				{
					var path = Path.Combine(ModSettings.GetConfigPath(), "config_migration");
					File.WriteAllText(path, "Remove this file if you want to see the migration dialog again.");
				    
					PUtil.LogDebug("Saved mark file that used have seen migration dialog.");
				};
				dialog.Body.AddChild(label);
				dialog.Show();
			}
		}
	}
}