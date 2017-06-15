@echo off

for /f "usebackq tokens=1* delims=: " %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere" -latest -requires Microsoft.Component.MSBuild`) do (
	if /i "%%i"=="installationPath" set InstallDir=%%j
)

set msbuild="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"

if not exist %msbuild% (
	echo Failed to locate MSBuild 15
	goto:eof
)

if not exist "nuget.exe" (
	echo Downloading nuget.exe...
	powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe"
)

nuget restore src\webdiff.csproj -SolutionDirectory src

%msbuild% src\webdiff.csproj /p:Configuration=Release /p:AllowedReferenceRelatedFileExtensions=none

xcopy /d /y cfg\profile.toml bin\
xcopy /d /y cfg\template.html bin\
xcopy /d /y cfg\cookies.txt bin\

xcopy /d /y ext\bin\webdiff.crx bin\

if not exist "bin\chromedriver_win32.zip" (
	echo Downloading chromedriver...
	powershell -Command "Invoke-WebRequest https://chromedriver.storage.googleapis.com/2.30/chromedriver_win32.zip -OutFile bin/chromedriver_win32.zip"
)

if not exist "bin\chromedriver.exe" (
	echo Unzipping chromedriver...
	powershell -Command "Expand-Archive bin/chromedriver_win32.zip -DestinationPath bin"
)

echo Done! Check bin directory!
echo Try: echo /^|webdiff https://example.com/prod https://example.com/test
