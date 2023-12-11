using System.IO;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;

namespace TemperatureThresholds.Patches
{
	// ReSharper disable UnusedType.Global
	public class StartupDialogsPatch
	{
		[HarmonyPatch(typeof(MainMenu))]
		[HarmonyPatch("OnSpawn")]
		public class ShowPopup
		{
			public static void Postfix()
			{
				ShowProfileErrorsPopup();
				ShowMigrationPopup();
			}
		}

		private static void ShowProfileErrorsPopup()
		{
			if (TemperatureProfiles.Instance.erroredProfiles.Count > 0)
			{
				var errorDialog = new PDialog("ProfileLoadError")
				{
					Title = "Temperature Thresholds"
				};
				errorDialog.Body.Margin = new UnityEngine.RectOffset(20, 20, 20, 20);
				errorDialog.Body.AddChild(new PLabel("ProfileLoadErrorLabel")
				{
					Text = "Could not load profiles:",
					TextStyle = PUITuning.Fonts.TextLightStyle
				});
				foreach (var errored in TemperatureProfiles.Instance.erroredProfiles)
				{
					errorDialog.Body.AddChild(new PLabel("ProfileLoadErrorLabelOf"+errored)
					{
						Text = errored,
						TextStyle = PUITuning.Fonts.TextLightStyle
					});
				}
				errorDialog.Body.AddChild(new PLabel("ProfileLoadErrorLabelEnd")
				{
					Text = "\nCheck player.log for more details.",
					TextStyle = PUITuning.Fonts.TextLightStyle
				});
				errorDialog.Show();
			}
		}
		
		private static void ShowMigrationPopup()
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