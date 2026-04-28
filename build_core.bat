@echo off
echo Building Core...

dotnet build Core\Core.csproj -c Debug

IF %ERRORLEVEL% NEQ 0 (
    echo Build failed.
    pause
    exit /b %ERRORLEVEL%
)

echo Copying DLL from %SOURCE% to %DEST%

set SOURCE=Core\bin\Debug\netstandard2.1\Core.dll
set DEST=UnityTwentyFive\Assets\Plugins\Generated\Core.dll

if not exist UnityTwentyFive\Assets\Plugins (
    mkdir UnityTwentyFive\Assets\Plugins
)

if not exist UnityTwentyFive\Assets\Plugins\Generated (
    mkdir UnityTwentyFive\Assets\Plugins\Generated
)

copy /Y %SOURCE% %DEST%

echo Done.
pause