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
msbuild -property:Configuration=Release;OutputPath=Bin\Release\Library;Platform="Any CPU" -restore -target:rebuild DigitalZenWorks.Database.ToolKit.csproj
msbuild -property:Configuration=Release;OutputPath=Bin\Release\Library;Platform="Any CPU" -restore -target:pack DigitalZenWorks.Database.ToolKit.csproj

cd Bin\Release\Library
nuget push DigitalZenWorks.Database.ToolKit.%2.nupkg %3 -Source https://api.nuget.org/v3/index.json
GOTO end

:end
