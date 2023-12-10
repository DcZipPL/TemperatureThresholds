using System.IO;
using static TemperatureThresholds.TemperatureProfiles;

namespace TemperatureThresholds
{
    internal class ModSettings
    {
        public static ModSettings Instance = new ModSettings();
        public TemperatureProfile? currentProfile;
        
        public static string GetConfigPath()
        {
            return Path.Combine(KMod.Manager.GetDirectory(), "config", "TemperatureThresholds");
        }
    }
}
