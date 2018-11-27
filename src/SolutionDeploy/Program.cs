namespace SolutionDeploy
{
    using Microsoft.Extensions.CommandLineUtils;

    public static class Program
    {
        private const string HelpOptionTemplate = "-? | -h | -help | --help";

        public static int Main(string[] args)
        {
            CommandLineApplication commandLineApplication =
                new CommandLineApplication();
            commandLineApplication.HelpOption(HelpOptionTemplate);
            commandLineApplication.Command("deploy", DeployCommand.Configure);

            int retVal = -1;
            if (args.Length == 0)
            {
                commandLineApplication.ShowHelp();
            }

            Configuration.Build();

            try
            {
                retVal = commandLineApplication.Execute(args);
            }
            catch (CommandParsingException)
            {
                commandLineApplication.ShowHelp();
            }

            return retVal;
        }
    }
}
