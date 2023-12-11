using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TemperatureThresholds.Patches
{
	// ReSharper disable UnusedType.Global
	public class ProfileSettingsPatch
	{
		// Settings Screen
		[HarmonyPatch(typeof(GameOptionsScreen))]
		[HarmonyPatch("OnSpawn")]
		public static class AddProfilesButton
		{
			public static void Prefix(GameOptionsScreen __instance)
			{
				var content = __instance.GetComponent<Transform>().Find("Content");
				if (content == null)
				{
					PUtil.LogWarning("Failed to find Content transform!");
					return;
				}
				var saveOptions = content.Find("SaveOptions");
				if (saveOptions == null)
				{
					PUtil.LogWarning("Failed to find Content/SaveOptions transform!");
					return;
				}
				var saveOptionsBackground = saveOptions.Find("BG");
				if (saveOptionsBackground == null)
				{
					PUtil.LogWarning("Failed to find Content/SaveOptions/BG transform!");
					return;
				}

				var sprite = saveOptionsBackground.gameObject.GetComponent<Image>().sprite;
				
				PUtil.LogDebug("Injecting button into settings.");
				var btn = new PButton
				{
					Text = "Open Temperature Profiles",
					OnClick = sender =>
					{
						PUtil.LogDebug("Tried to open mod directory!");
						Process.Start(ModSettings.GetConfigPath());
					},
					ToolTip = "Opens TemperatureThresholds config directory",
					Margin = new RectOffset(10,10,10,10)
				};
				btn.SetKleiBlueStyle();

				var labelStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
				labelStyle.textColor = PUITuning.Fonts.UILightStyle.textColor;
				labelStyle.style = PUITuning.Fonts.UILightStyle.style;
				labelStyle.sdfFont = PUITuning.Fonts.UILightStyle.sdfFont;
				labelStyle.enableWordWrapping = true;
				labelStyle.fontSize = 24;
				
				PPanel buttonPanel = new PPanel("TemperatureThresholdsButtonPanel")
				{
					Margin = new RectOffset(0, 0, 16, 0),
					FlexSize = Vector2.left,
					Direction = PanelDirection.Horizontal
				};
				buttonPanel.AddChild(btn);
				
				PLabel header = new PLabel("TemperatureThresholdsHeader")
				{
					Text = "Overlay",
					TextStyle = labelStyle,
					TextAlignment = TextAnchor.LowerLeft,
				};
				
				PPanel panel = new PPanel("TemperatureThresholdsPanel")
				{
					Margin = new RectOffset(12, 12, 12, 12),
					FlexSize = Vector2.left,
					BackColor = new Color(0.188f, 0.204f, 0.263f, 1.0f),
					BackImage = sprite,
					ImageMode = Image.Type.Sliced,
					Alignment = TextAnchor.MiddleLeft,
					Direction = PanelDirection.Vertical
				};
				panel.AddChild(header);
				panel.AddChild(buttonPanel);
				
				GameObject go = panel.AddTo(content.gameObject);
				go.transform.SetSiblingIndex(2);
				go.SetActive(true);
			}
		}
	}
}