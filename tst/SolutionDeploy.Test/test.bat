@echo off

SET dotnet="dotnet.exe"  

SET opencoverVersion=4.6.519
SET reportgeneratorVersion=3.1.2
SET opencoverConverterVersion=0.3.2
SET opencover="%USERPROFILE%\.nuget\packages\OpenCover\%opencoverVersion%\tools\OpenCover.Console.exe"
SET reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\%reportgeneratorVersion%\tools\ReportGenerator.exe"
SET opencoverConverter="%USERPROFILE%\.nuget\packages\OpenCoverToCoberturaConverter\%opencoverConverterVersion%\tools\OpenCoverToCoberturaConverter.exe"

SET targetargs="test tst\SolutionDeploy.Test\SolutionDeploy.Test.csproj --configuration Test --logger trx;LogFileName=SolutionDeploy_Result.trx --no-restore"  
SET filter="+[SolutionDeploy]* +[SolutionDeploy.*]* -[*.Test]* -[*.Test.Integration]* -[xunit.*]* -[FluentValidation]* -[SolutionDeploy]SolutionDeploy.Program -[SolutionDeploy]SolutionDeploy.Startup -[SolutionDeploy]SolutionDeploy.ServiceProvider -[SolutionDeploy]SolutionDeploy.Configuration -[SolutionDeploy]SolutionDeploy.DeployCommand -[SolutionDeploy.Vsts]SolutionDeploy.Vsts.VstsSyncReleaseClient -[SolutionDeploy]SolutionDeploy.FileSystem -[SolutionDeploy.Core]SolutionDeploy.Core.HttpClient -[SolutionDeploy.Core]SolutionDeploy.Core.Logging -[SolutionDeploy.Core]SolutionDeploy.Core.AuthenticationResult -[SolutionDeploy.Core]SolutionDeploy.Core.OAuthAccessTokens -[SolutionDeploy.Core]SolutionDeploy.Core.VstsConfig -[SolutionDeploy.Core]SolutionDeploy.Core.NoTokensException -[SolutionDeploy.Core]SolutionDeploy.Core.Options" 
SET outputfile=tst\SolutionDeploy.Test\coverage.xml
SET coveragedir=tst\SolutionDeploy.Test\coverage
SET coberturaCoverageFile=%coveragedir%\cobertura-coverage.xml

REM Run code coverage analysis  
%opencover% -oldStyle -register:user -target:%dotnet% -output:%outputfile% -filter:%filter% -targetargs:%targetargs% -skipautoprops

REM Generate the Cobertura report
%opencoverConverter% -input:%outputfile% -output:%coberturaCoverageFile% -sources:..

REM Generate the readable reports
%reportgenerator% -targetdir:%coveragedir% -reporttypes:HtmlInline;TextSummary -reports:%outputfile% -verbosity:Error

REM Read the summary
TYPE %coveragedir%\Summary.txt
