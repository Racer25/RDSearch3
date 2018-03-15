using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigurationJSON
{
    public class ConfigurationManager
    {
        public static dynamic GetSetting(string setting)
        {
            //Trouver le fichier de configuration
            // Config (provided through a settings.json file)
            var execution_path = AppDomain.CurrentDomain.BaseDirectory;
            
            if (execution_path.EndsWith("/bin/Debug/"))
                execution_path = execution_path.Replace("/bin/Debug", "/..");
            if (execution_path.EndsWith(@"\bin\Debug\"))
                execution_path = execution_path.Replace(@"\bin\Debug", @"\..");


            var user_config_filename = "settings.json";

            using (StreamReader r = new StreamReader(execution_path + "/"+user_config_filename))
            {
                Config config = JsonConvert.DeserializeObject<Config>(r.ReadToEnd());

                return config.GetType().GetProperty(setting).GetValue(config, null);
            }
        }
    }
}
