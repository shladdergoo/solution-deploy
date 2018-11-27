namespace SolutionDeploy
{
    using System;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    internal static class ServiceProvider
    {
        private static IServiceProvider serviceProvider;

        public static void Build(Options options)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            AddLogging(serviceCollection);

            AddServices(serviceCollection, options);

            serviceProvider = serviceCollection.BuildServiceProvider();

            Logging.Build(serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        public static T GetService<T>()
        {
            if (serviceProvider == null)
            {
                Build(null);
            }

            return serviceProvider.GetService<T>();
        }

        public static void Dispose()
        {
            ((IDisposable)serviceProvider).Dispose();
        }

        private static void AddLogging(IServiceCollection serviceCollection)
        {
            if (Configuration.Logging != null)
            {
                serviceCollection.AddLogging(config =>
                    config.AddConfiguration(Configuration.Logging).AddConsole());
            }
            else
            {
                serviceCollection.AddLogging(config => config.AddConsole());
            }
        }

        private static void AddServices(IServiceCollection serviceCollection, Options options)
        {
            serviceCollection
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IHttpClient, HttpClient>()
                .AddSingleton<IProductManifestRepository, JsonFileProductManifestRepository>()
                .AddSingleton<ITokenRepository, JsonFileTokenRepository>()
                .AddSingleton<IVstsReleaseClient, VstsSyncReleaseClient>(
                    (ctx) =>
                    {
                        return new VstsSyncReleaseClient(Configuration.VstsConfig, options);
                    })
                .AddSingleton<IReleaseRepository, VstsReleaseRepository>(
                    (ctx) =>
                    {
                        IVstsReleaseClient releaseClient = ctx.GetService<IVstsReleaseClient>();
                        IAuthenticator authenticator = ctx.GetService<IAuthenticator>();
                        return new VstsReleaseRepository(
                            releaseClient, authenticator, Configuration.VstsConfig);
                    })
                .AddSingleton<IAuthenticator, VstsOAuthAuthenticator>(
                    (ctx) =>
                    {
                        IHttpClient httpClient = ctx.GetService<IHttpClient>();
                        ITokenRepository tokenRepository = ctx.GetService<ITokenRepository>();
                        return new VstsOAuthAuthenticator(
                            httpClient, tokenRepository, Configuration.VstsConfig);
                    })
                .AddSingleton<IServiceDeploymentExecutor, VstsDeploymentExecutor>(
                    (ctx) =>
                    {
                        IReleaseRepository repository = ctx.GetService<IReleaseRepository>();
                        return
                            new VstsDeploymentExecutor(repository, Configuration.VstsConfig, options);
                    })
                .AddSingleton<IServiceDeploymentHandler, SequentialDeploymentHandler>(
                    (ctx) =>
                    {
                        IServiceDeploymentExecutor executor = ctx.GetService<IServiceDeploymentExecutor>();
                        return new SequentialDeploymentHandler(executor, 10, options);
                    })
                .AddSingleton<IDeploymentService, DeploymentService>();
        }
    }
}
