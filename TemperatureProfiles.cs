using System;
using System.Collections.Generic;
using System.IO;
using PeterHan.PLib.Core;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TemperatureThresholds
{
    internal class TemperatureProfiles
    {
        public static TemperatureProfiles Instance = new TemperatureProfiles();

        public List<TemperatureProfile> profiles = new List<TemperatureProfile>();
        public List<string> erroredProfiles = new List<string>();

        public class PColor
        {
            public float r { get; set; }
            public float g { get; set; }
            public float b { get; set; }
            public float a { get; set; }

            public static implicit operator Color(PColor p) => new Color(p.r / 255f, p.g / 255f, p.b / 255f, p.a / 255f);
        }

        public struct ColorThreshold
        {
            public PColor color { get; set; }
            public float temperature { get; set; }
            public string legendName { get; set; }
            public string legendDescription { get; set; }
        }

        public struct TemperatureProfile
        {
            public string id { get; set; }
            public string name { get; set; }
            public ColorThreshold[] thresholds { get; set; }
        }

        public bool Load()
        {
            try
            {
                var path = ModSettings.GetConfigPath();
                PUtil.LogDebug($"Default config directory: {path}");
                
                if (!Directory.Exists(path))
                {
                    PUtil.LogDebug($"Creating config directory: {path}");
                    Directory.CreateDirectory(path);
                }
                
                foreach (string filePath in Directory.EnumerateFiles(path))
                {
                    if (Path.GetExtension(filePath) == ".yaml" || Path.GetExtension(filePath) == ".yml")
                    {
                        try
                        {
                            string yml = File.ReadAllText(filePath);
                            var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(new CamelCaseNamingConvention())
                                .Build();

                            this.profiles.Add(deserializer.Deserialize<TemperatureProfile>(yml));
                            PUtil.LogDebug($"Loaded: {deserializer.Deserialize<TemperatureProfile>(yml).name}");
                        }
                        catch (Exception ex)
                        {
                            PUtil.LogWarning($"Could not load {filePath}:\n{ex}");
                            erroredProfiles.Add(Path.GetFileNameWithoutExtension(filePath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PUtil.LogError($"Temperature Profile error:\n===============\n{ex}\n===============\n");
                return false;
            }
            return true;
        }

        internal static bool InFilter(string layer, Dictionary<string, ToolParameterMenu.ToggleState> filter)
        {
            if (filter.ContainsKey(ToolParameterMenu.FILTERLAYERS.ALL) && filter[ToolParameterMenu.FILTERLAYERS.ALL] == ToolParameterMenu.ToggleState.On)
                return true;
            return filter.ContainsKey(layer) && filter[layer] == ToolParameterMenu.ToggleState.On;
        }
    }
}
