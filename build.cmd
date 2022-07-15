REM %1 - Type of build
REM %2 - Version (such as 1.0.0.5)
REM %3 - API key

CD %~dp0
CD DatabaseLibraryNet

IF "%1"=="publish" GOTO publish

:default
dotnet build

GOTO end

:publish

msbuild -property:Configuration=Release;OutputPath=Bin\Release -restore  DigitalZenWorks.Database.ToolKit.csproj
cd Bin\Release
nuget push DigitalZenWorks.Database.ToolKit.%2.nupkg %3 -Source https://api.nuget.org/v3/index.json
GOTO end

:end
