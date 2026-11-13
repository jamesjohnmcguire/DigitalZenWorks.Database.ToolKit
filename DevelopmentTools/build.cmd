@ECHO OFF
SETLOCAL
SET "buildExitCode=0"

CD %~dp0
CD ..\DatabaseLibraryNet

IF /I "%~1"=="publish" GOTO publish

:default
dotnet build --configuration Release -p:PlatformTarget=AnyCPU -p:Prefer32Bit=false

GOTO :EOF

:publish
WHERE gh >NUL 2>&1
SET "buildExitCode=%ERRORLEVEL%"
IF NOT "%buildExitCode%"=="0" SET "message=GitHub CLI is required. Install gh and run gh auth login."
IF NOT "%buildExitCode%"=="0" GOTO error

:: Get Project Version
SET "projectVersion="
FOR /F "delims=" %%V IN (
    'dotnet msbuild -nologo -getProperty:Version'
) DO SET "projectVersion=%%V"

IF NOT DEFINED projectVersion SET "message=Unable to read the project version."
IF NOT DEFINED projectVersion GOTO error

ECHO Requesting publication of version %projectVersion% from main.
gh workflow run dotnet.yml --ref main -F publish=true -f version="%projectVersion%" --repo jamesjohnmcguire/DigitalZenWorks.Database.ToolKit

SET "buildExitCode=%ERRORLEVEL%"
IF NOT "%buildExitCode%"=="0" SET "message=Problem Running: gh workflow run dotnet.yml"
IF NOT "%buildExitCode%"=="0" GOTO error

ECHO Workflow requested. GitHub will build, test, and publish the package.
ECHO Use gh run watch from the Database.ToolKit repository to follow its progress.
GOTO :EOF

:error
ECHO %message%
