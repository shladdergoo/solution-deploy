namespace SolutionDeploy.Core
{
    using Microsoft.Extensions.Logging;

    public static class Logging
    {
        private static ILoggerFactory loggerFactory;

        public static void Build(ILoggerFactory loggerFactory)
        {
            Logging.loggerFactory = loggerFactory;
        }

        public static ILogger GetLogger<T>()
        {
            if(loggerFactory == null)
            {
                return new LoggerFactory().AddConsole().CreateLogger<T>();
            }

            return loggerFactory.CreateLogger<T>();
        }
    }
}
