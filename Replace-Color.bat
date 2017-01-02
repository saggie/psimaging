@echo off

if "%~1" == "" echo Please specify the file path. & pause & exit

set THIS_DIRECTORY_PATH=%~dp0
set PROCESSING_TYPE=%~n0
set IMAGE_FILE_PATH=%~1

set /P COLOR_FROM="Input the color to replace (from): "
set /P COLOR_TO="Input the color to replace (to): "

PowerShell -File "%THIS_DIRECTORY_PATH%scripts\Do-Imaging.ps1" -ProcessingType "%PROCESSING_TYPE%" -SourceImageFilePath "%IMAGE_FILE_PATH%"  -Option1 "%COLOR_FROM%"  -Option2 "%COLOR_TO%"
