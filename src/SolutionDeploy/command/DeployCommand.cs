namespace SolutionDeploy
{
    using System;

    using Microsoft.Extensions.CommandLineUtils;

    using SolutionDeploy.Core;

    internal class DeployCommand
    {
        private const string HelpOptionTemplate = "-? | -h | -help | --help";

        public static void Configure(CommandLineApplication command)
        {
            CommandArgument productName = command.Argument(
                "productName",
                "The name of the product to be deployed");

            CommandArgument environment = command.Argument(
                "environment",
                "The name of the deployment environment");

            CommandOption productVersion = command.Option(
                "-v | --version",
                "The version number of the product to be deployed",
                CommandOptionType.SingleValue);

            CommandOption branch = command.Option(
                "-b |--branch",
                "The name of the code branch to be used for deployment",
                CommandOptionType.SingleValue);

            CommandOption allowPartial = command.Option(
                "-p | --allow-partial",
                "Consider partially succeeded releases to pre-req environment",
                CommandOptionType.NoValue);

            CommandOption whatIf = command.Option(
                "-w | --what-if",
                "Run in 'what-if' mode. No releases will be deployed",
                CommandOptionType.NoValue);

            command.HelpOption(HelpOptionTemplate);

            command.OnExecute(() =>
                {
                    if (productName.Value == null || environment.Value == null)
                    {
                        command.ShowHelp();
                    }
                    else
                    {
                        Options options = BuildOptions(whatIf.HasValue(), allowPartial.HasValue());
                        ServiceProvider.Build(options);

                        try
                        {
                            ServiceProvider.GetService<IDeploymentService>()
                                .Deploy(
                                    productName.Value,
                                    environment.Value,
                                    productVersion.Value(),
                                    branch.Value());
                        }
                        catch (NoTokensException ex)
                        {
                            WriteNoTokensMessage(ex.AuthorizationUrl);
                            return -1;
                        }
                        catch
                        {
                            return -1;
                        }
                        finally
                        {
                            ServiceProvider.Dispose();
                        }
                    }
                    return 0;
                });
        }

        private static Options BuildOptions(bool whatif, bool allowPartial)
        {
            if (whatif || allowPartial)
            {
                return new Options
                {
                    WhatIf = whatif,
                    AllowPartial = allowPartial
                };
            }

            return null;
        }

        private static void WriteNoTokensMessage(Uri authorizationUrl)
        {
            Console.WriteLine();
            Console.WriteLine("Application has not been authorized.");
            Console.WriteLine("Please visit the following url to authorize the application:");
            Console.WriteLine(authorizationUrl);
            Console.WriteLine();
        }
    }
}
