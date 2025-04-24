using System;
using System.IO;
using System.Xml.Serialization;

namespace EmployerTimeManagement.Themes
{
    [Serializable]
    public class ThemeSettings
    {
        public string SelectedTheme { get; set; } = "ModernDark";
    }

    public static class ThemeConfig
    {
        private static readonly string FilePath = "ThemeSettings.xml";

        public static ThemeSettings Load()
        {
            if (!File.Exists(FilePath))
                return new ThemeSettings();

            try
            {
                using var stream = new FileStream(FilePath, FileMode.Open);
                var serializer = new XmlSerializer(typeof(ThemeSettings));
                return (ThemeSettings)serializer.Deserialize(stream);
            }
            catch
            {
                return new ThemeSettings();
            }
        }

        public static void Save(ThemeSettings settings)
        {
            using var stream = new FileStream(FilePath, FileMode.Create);
            var serializer = new XmlSerializer(typeof(ThemeSettings));
            serializer.Serialize(stream, settings);
        }
    }
}
