using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using TMPro;
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
			//static AccessTools.FieldRef<UnitConfigurationScreen, GameObject> toggleGroupRef =
			//	AccessTools.FieldRefAccess<UnitConfigurationScreen, GameObject>("toggleGroup");

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
					Text = "Open Overlay Profiles",
					OnClick = sender =>
					{
						PUtil.LogDebug("Tried to open mod directory!");
						Process.Start(ModSettings.GetConfigPath());
					},
					ToolTip = "Opens TemperatureThresholds config directory"
				};
				btn.SetKleiBlueStyle();

				var labelStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
				labelStyle.textColor = PUITuning.Fonts.UILightStyle.textColor;
				labelStyle.style = PUITuning.Fonts.UILightStyle.style;
				labelStyle.sdfFont = PUITuning.Fonts.UILightStyle.sdfFont;
				labelStyle.enableWordWrapping = true;
				labelStyle.fontSize = 24;
				
				PLabel label = new PLabel("TemperatureThresholdsLabel")
				{
					Text = "Overlay",
					TextStyle = labelStyle,
					TextAlignment = TextAnchor.LowerLeft,
				};
				var labelTransform = label.Build().GetComponent<RectTransform>();
				
				PPanel labelPanel = new PPanel("TemperatureThresholdsLabelPanel")
				{
					Direction = PanelDirection.Horizontal,
					Spacing = 0,
					Alignment = TextAnchor.LowerLeft
				};
				labelPanel.AddChild(label);
				
				PGridPanel panel = new PGridPanel("TemperatureThresholdsPanel")
				{
					Margin = new RectOffset(12, 12, 12, 12),
					FlexSize = Vector2.left,
					BackColor = new Color(0.188f, 0.204f, 0.263f, 1.0f),
					BackImage = sprite,
					ImageMode = Image.Type.Sliced,
				};
				panel.AddRow(new GridRowSpec());
				panel.AddRow(new GridRowSpec());
				panel.AddColumn(new GridColumnSpec());
				panel.AddChild(labelPanel, new GridComponentSpec(0, 0));
				panel.AddChild(btn, new GridComponentSpec(1, 0));
				
				GameObject go = panel.AddTo(content.gameObject);
				go.transform.SetSiblingIndex(2);
				go.SetActive(true);
				
				printTreeOfChildren(content);
				printTreeOfChildren(go.transform);
			}
		}
		
		public static Transform printTreeOfChildren(Transform transform, int depth = 0)
		{
			string indent = "";
			for (int i = 0; i < depth; i++)
			{
				indent += "  ";
			}
			PUtil.LogDebug(indent + transform.gameObject.name);
			
			// Get component names of SaveOptions and values
			Component[] components = transform.gameObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				string componentName = component.GetType().Name;
				PUtil.LogDebug(indent + "& " + componentName);
			
				FieldInfo[] fields = component.GetType().GetFields();
				PropertyInfo[] properties = component.GetType().GetProperties();

				foreach (FieldInfo field in fields)
				{
					object value = field.GetValue(component);
					PUtil.LogDebug(indent + "&F> " + "Name: " + field.Name + ", Value: " + value);
				}

				try
				{
					foreach (PropertyInfo property in properties)
					{
						/*if (property.Name == "sprite")
						{
							Sprite sprite = property.GetValue(component) as Sprite;
							var texture = sprite.texture;
							PUtil.LogDebug(indent + "! Texture: \n" + texture.name + "\n" + texture.width + "x" + texture.height);
							break;
						}*/
						try
						{
							object value = property.GetValue(component);
							PUtil.LogDebug(indent + "&P> " + "Name: " + property.Name + ", Value: " + value);
						} catch (System.Exception e)
						{
							PUtil.LogDebug(indent + "&P1> " + "Exception: " + e.Message);
						}
					}
				} catch (System.Exception e)
				{
					PUtil.LogDebug(indent + "&P0> " + "Exception: " + e.Message);
				}
			}
			
			foreach (Transform child in transform)
			{
				printTreeOfChildren(child, depth + 1);
			}
			return transform;
		}
	}
}