namespace SolutionDeploy
{
    using System;
    using System.IO;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;

    using SolutionDeploy.Core;

    internal static class Configuration
    {
        private const string ConfigFile = "appsettings.json";

        private static VstsConfig vstsConfig;
        private static IConfigurationSection logging;

        public static VstsConfig VstsConfig
        {
            get
            {
                return vstsConfig;
            }
        }

        public static IConfigurationSection Logging
        {
            get
            {
                return logging;
            }
        }

        public static void Build()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(ConfigFile);

            IConfiguration configuration = builder.Build();

            logging = configuration.GetSection("Logging");
            var children = logging.GetChildren();

            vstsConfig = new VstsConfig();
            configuration.Bind("Vsts", vstsConfig);
        }
    }
}
