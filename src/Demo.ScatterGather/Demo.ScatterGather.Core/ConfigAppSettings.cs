using System.Configuration;

namespace Demo.ScatterGather.Core
{
    public class ConfigAppSettings : IAppSettings
    {
        public string Get(string key, bool required = false)
        {
            var value = ConfigurationManager.AppSettings[key];

            if (required && string.IsNullOrEmpty(value))
            {
                throw new ConfigurationErrorsException($"No value was specified for the key '{key}'.");
            }

            return value;
        }
    }
}