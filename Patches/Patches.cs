using System;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PeterHan.PLib.Core;
using UnityEngine;
using static TemperatureThresholds.TemperatureProfiles;
using static STRINGS.UI.OVERLAYS;

namespace TemperatureThresholds.Patches
{
	// ReSharper disable UnusedType.Global
    public class Patches
    {
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public class LoadProfiles
        {
            public static void Prefix()
            {
	            PUtil.LogDebug("Loading your Temperature Profiles..."); ;
	            if (Instance.Load()) // Check if loads successfully
		            PUtil.LogDebug("Loaded Temperature Profiles successfully!");
	            else
		            PUtil.LogWarning("Failed to load Temperature Profiles!");
            }
        }

        [HarmonyPatch(typeof(OverlayModes.Temperature))]
        [HarmonyPatch("CreateDefaultFilters")]
        public class AddMoreFiltersToOverlay
        {
            public static void Postfix(ref Dictionary<string, ToolParameterMenu.ToggleState> __result)
            {
	            PUtil.LogDebug($"Adding filters (0/{Instance.profiles.Count})...");
				OrderedDictionary od = new OrderedDictionary();
                foreach (var pair in __result)
                {
                    od.Add(pair.Key, pair.Value);
                }
                foreach (TemperatureProfile profile in Instance.profiles)
                {
                    od.Insert(1, profile.id, ToolParameterMenu.ToggleState.Off);
                }
                __result.Clear();
                __result = od.Cast<DictionaryEntry>().ToDictionary(k => (string)k.Key, v => (ToolParameterMenu.ToggleState)v.Value);
				PUtil.LogDebug("Filters added.");
			}
        }

        [HarmonyPatch(typeof(OverlayModes.Temperature))]
        [HarmonyPatch("OnFiltersChanged")]
        public class ApplyModFiltersWhenChanged
        {
            public static void Postfix(OverlayModes.Temperature __instance)
            {
                foreach (TemperatureProfile profile in Instance.profiles)
                {
	                if (InFilter(profile.id, __instance.legendFilters))
                    {
                        Game.Instance.temperatureOverlayMode = Game.TemperatureOverlayModes.AbsoluteTemperature;
                        ModSettings.Instance.currentProfile = profile;
                        break;
                    }
	                ModSettings.Instance.currentProfile = null;
                }
            }
        }
        
        [HarmonyPatch(typeof(OverlayModes.Temperature))]
        [HarmonyPatch("Enable")]
        public class FixOverlayEnableCrash
        {
	        public static bool Prefix(OverlayModes.Temperature __instance)
	        {
		        PUtil.LogDebug($"OverlayModes.Temperature.Enable() called. This part of code is disabled. If you have issues with this part of code, please report it on GitHub.");
		        /*PUtil.LogDebug($"Debug: {SimDebugView.Instance.temperatureThresholds.Length} {__instance.temperatureLegend.Count}");
		        int num = SimDebugView.Instance.temperatureThresholds.Length - 1;
		        for (int index = 0; index < __instance.temperatureLegend.Count; ++index)
		        {
			        __instance.temperatureLegend[index].colour = (Color) GlobalAssets.Instance.colorSet.GetColorByName(SimDebugView.Instance.temperatureThresholds[num - index].colorName);
			        __instance.temperatureLegend[index].desc_arg = GameUtil.GetFormattedTemperature(SimDebugView.Instance.temperatureThresholds[num - index].value);
		        }*/
		        // Disable this part of code. I have no idea what it does, but it crashes the game.
		        return false;
	        }
        }

		[HarmonyPatch(typeof(OverlayModes.Temperature))]
		[HarmonyPatch("GetCustomLegendData")]
		public class ApplyNewLegendData
		{
			public static void Postfix(List<LegendEntry> __result)
			{
				if (Game.Instance.temperatureOverlayMode == Game.TemperatureOverlayModes.AbsoluteTemperature)
                {
					__result.Clear();
					if (ModSettings.Instance.currentProfile != null)
					{
						foreach (ColorThreshold colorThreshold in ModSettings.Instance.currentProfile?.thresholds)
						{
							string legendName;
							string legendDescription;
							switch (colorThreshold.legendName)
							{
								case "$MAXHOT":
									legendName = TEMPERATURE.MAXHOT;
									break;
								case "$EXTREMEHOT":
									legendName = TEMPERATURE.EXTREMEHOT;
									break;
								case "$VERYHOT":
									legendName = TEMPERATURE.VERYHOT;
									break;
								case "$HOT":
									legendName = TEMPERATURE.HOT;
									break;
								case "$TEMPERATE":
									legendName = TEMPERATURE.TEMPERATE;
									break;
								case "$COLD":
									legendName = TEMPERATURE.COLD;
									break;
								case "$VERYCOLD":
									legendName = TEMPERATURE.VERYCOLD;
									break;
								case "$EXTREMECOLD":
									legendName = TEMPERATURE.EXTREMECOLD;
									break;
								default:
									legendName = colorThreshold.legendName;
									break;
							}

							if (colorThreshold.legendDescription == "$DEFAULT")
							{
								legendDescription = string.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(colorThreshold.temperature)) + GameUtil.GetTemperatureUnitSuffix();
							}
							else
							{
								legendDescription = colorThreshold.legendDescription;
							}

							__result.Add(new LegendEntry(legendName, legendDescription, (Color)colorThreshold.color));
						}
					}
					else
					{
						// TODO: Read default values
						__result.Add(new LegendEntry(TEMPERATURE.MAXHOT, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(2073.15f) + GameUtil.GetTemperatureUnitSuffix()), new Color(0.8901961f, 0.1372549f, 0.1294118f)));
						__result.Add(new LegendEntry(TEMPERATURE.EXTREMEHOT, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(373.15f) + GameUtil.GetTemperatureUnitSuffix()), new Color(0.9843137f, 0.3254902f, 0.3137255f)));
						__result.Add(new LegendEntry(TEMPERATURE.VERYHOT, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(310.15f) + GameUtil.GetTemperatureUnitSuffix()), new Color(1f, 0.6627451f, 0.1411765f)));
						__result.Add(new LegendEntry(TEMPERATURE.HOT, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(300.15f) + GameUtil.GetTemperatureUnitSuffix()), new Color(0.9372549f, 1f, 0.0f)));
						__result.Add(new LegendEntry(TEMPERATURE.TEMPERATE, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(293.15f) + GameUtil.GetTemperatureUnitSuffix()), new Color(0.2313726f, 0.9960784f, 0.2901961f)));
						__result.Add(new LegendEntry(TEMPERATURE.COLD, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(283.15f) + GameUtil.GetTemperatureUnitSuffix()), new Color(0.1215686f, 0.6313726f, 1f)));
						__result.Add(new LegendEntry(TEMPERATURE.VERYCOLD, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(273.15f) + GameUtil.GetTemperatureUnitSuffix()), new Color(0.1686275f, 0.7960784f, 1f)));
						__result.Add(new LegendEntry(TEMPERATURE.EXTREMECOLD, String.Format(TEMPERATURE.TOOLTIPS.TEMPERATURE, GameUtil.GetConvertedTemperature(0f) + GameUtil.GetTemperatureUnitSuffix()), new Color(0.5019608f, 0.9960784f, 0.9411765f)));
					}
				}
            }
		}

		[HarmonyPatch(typeof(SimDebugView))]
		[HarmonyPatch("NormalizedTemperature")]
		public class SimDebugView_NormalizedTemperature_Patch
		{
			public static void Postfix(SimDebugView __instance, float actualTemperature, ref Color __result)
			{
				if (ModSettings.Instance.currentProfile != null)
				{
					ColorThreshold[] thresholds = ModSettings.Instance.currentProfile?.thresholds;
					if (thresholds == null || thresholds.Length == 0)
						return;

					int index1 = 0;
					int index2 = 0;
					for (int index3 = 0; index3 < ModSettings.Instance.currentProfile?.thresholds.Length; ++index3)
					{
						if ((double)actualTemperature <= (double)thresholds[index3].temperature)
						{
							index2 = index3;
							break;
						}
						index1 = index3;
						index2 = index3;
					}
					float a = 0.0f;
					if (index1 != index2)
						a = (float)(((double)actualTemperature - (double)thresholds[index1].temperature) / ((double)thresholds[index2].temperature - (double)thresholds[index1].temperature));
					float t = Mathf.Min(Mathf.Max(a, 0.0f), 1f);
					__result = Color.Lerp((Color)thresholds[index1].color, (Color)thresholds[index2].color, t);
				}
				else
				{
					int index1 = 0;
					int index2 = 0;
					for (int index3 = 0; index3 < __instance.temperatureThresholds.Length; ++index3)
					{
						if ((double)actualTemperature <= (double)__instance.temperatureThresholds[index3].value)
						{
							index2 = index3;
							break;
						}
						index1 = index3;
						index2 = index3;
					}
					float a = 0.0f;
					if (index1 != index2)
						a = (float)(((double)actualTemperature - (double)__instance.temperatureThresholds[index1].value) / ((double)__instance.temperatureThresholds[index2].value - (double)__instance.temperatureThresholds[index1].value));
					float t = Mathf.Min(Mathf.Max(a, 0.0f), 1f);
					__result = Color.Lerp((Color)GlobalAssets.Instance.colorSet.GetColorByName(__instance.temperatureThresholds[index1].colorName), (Color)GlobalAssets.Instance.colorSet.GetColorByName(__instance.temperatureThresholds[index2].colorName), t);
				}
			}
		}

		[HarmonyPatch(typeof(OverlayMenu))]
		[HarmonyPatch("OnSpawn")]
		public static class SetProfilesNames
		{
			public static void Postfix()
			{
				foreach (TemperatureProfile profile in Instance.profiles)
				{
					Strings.Add("STRINGS.UI.TOOLS.FILTERLAYERS."+profile.id, "Temperature ("+profile.name+")");
				}
			}
		}
	}
}
