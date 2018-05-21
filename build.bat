@echo off

for /f "usebackq tokens=1* delims=: " %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere" -latest -requires Microsoft.Component.MSBuild`) do (
	if /i "%%i"=="installationPath" set InstallDir=%%j
)

set msbuild="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"

if not exist %msbuild% (
	echo Failed to locate MSBuild 15
	goto:eof
)

mkdir tools 2>nul

if not exist "tools\nuget.exe" (
	echo Downloading nuget.exe...
	powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile tools\nuget.exe"
)

tools\nuget restore src\webdiff.csproj -SolutionDirectory src
tools\nuget install ILRepack -Version 2.0.15 -OutputDirectory tools

%msbuild% src\webdiff.csproj /p:Configuration=Release /p:AllowedReferenceRelatedFileExtensions=none

tools\ILRepack.2.0.15\tools\ILRepack /wildcards /lib:bin\ /out:bin\webdiff.exe bin\webdiff.exe bin\*.dll
del bin\*.dll
del bin\*.pdb
del bin\*.xml 2>nul

xcopy /d /y cfg\profile.toml bin\
xcopy /d /y cfg\template.html bin\
xcopy /d /y cfg\cookies.txt bin\

xcopy /d /y ext\bin\webdiff.crx bin\

if not exist "bin\chromedriver.exe" (
	echo Downloading chromedriver...
	powershell -Command "Invoke-WebRequest https://chromedriver.storage.googleapis.com/2.38/chromedriver_win32.zip -OutFile bin/chromedriver_win32.zip"

	echo Unzipping chromedriver...
	powershell -Command "Expand-Archive bin/chromedriver_win32.zip -DestinationPath bin"
)

del bin\chromedriver_win32.zip 2>nul

echo ==========================
echo Done! Try it:
echo echo .^|bin\webdiff https://github.com/dscheg/webdiff/tree/9f540f30aa69408485577590b9441211c8b202d1/ https://github.com/dscheg/webdiff/tree/a46231736f67a60538a59dd4c252f8f6be2d9bf7/
