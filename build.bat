@echo off
REM Build script for SWGSurvivors Patcher (C# version)

echo Building SWGSurvivors Patcher...
echo.

REM Clean previous builds
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

REM Build as single-file self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish

echo.
echo Build complete!
echo Executable location: publish\SWGSurvivors-Patcher.exe
echo.
pause
