REM %1 - Type of build
REM %2 - Version (such as 1.0.0.5)
REM %3 - API key

CD %~dp0
CD DatabaseLibraryNet

IF "%1"=="publish" GOTO publish

:default
dotnet build --configuration Release

GOTO end

:publish

if "%~2"=="" GOTO error1
if "%~3"=="" GOTO error2

msbuild -property:Configuration=Release -restore -target:rebuild;pack DigitalZenWorks.Database.ToolKit.csproj

CD bin\Release

nuget push DigitalZenWorks.Database.ToolKit.%2.nupkg %3 -Source https://api.nuget.org/v3/index.json
GOTO end

:error1
ECHO No version tag specified
GOTO end

:error2
ECHO No API key specified

:end
cd ..
