using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ConfigBatchConverter
{
    public class Program
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(FanControlConfig));

        public static void Main(string[] args)
        {
            string src = @"D:\Users\Stefan\Skydrive\Öffentlich\NBFC\Configs (V1)";
            string dst = @"C:\Program Files\NBFC Service\Configs";

            var dstManager = new FanControlConfigManager(dst);

            foreach (string s in Directory.GetFiles(src, "*.config"))
            {
                FanControlConfig cfg = null;

                if (TryLoadFanControlConfigV1(s, out cfg))
                {
                    try
                    {
                        dstManager.AddConfig(new FanControlConfigV2(cfg), cfg.UniqueId);
                    }
                    catch 
                    { 
                    }
                }
            }
        }

        private static bool TryLoadFanControlConfigV1(string configFilePath, out FanControlConfig config)
        {
            using (FileStream fs = new FileStream(configFilePath, FileMode.Open))
            {
                try
                {
                    config = (FanControlConfig)serializer.Deserialize(fs);
                }
                catch
                {
                    config = null;
                    return false;
                }
            }

            return config != null;
        }
    }
}
