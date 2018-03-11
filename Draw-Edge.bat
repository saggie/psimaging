@echo off

if "%~1" == "" echo Please specify the file path. & pause & exit

set THIS_DIRECTORY_PATH=%~dp0
set PROCESSING_TYPE=%~n0
set IMAGE_FILE_PATH=%~1

set /P LEVEL="Input the edge thickness (between 0-4): "
set /P DRAWING_COLOR="Input the drawing color: "
set /P BACKGROUND_COLOR="Input the background color: "

PowerShell -File "%THIS_DIRECTORY_PATH%scripts\Do-Imaging.ps1" ^
  -ProcessingType "%PROCESSING_TYPE%" ^
  -SourceImageFilePath "%IMAGE_FILE_PATH%" ^
  -Option1 %LEVEL% ^
  -Option2 "%DRAWING_COLOR%" ^
  -Option3 "%BACKGROUND_COLOR%"
