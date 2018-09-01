@echo off

if "%~1" == "" echo Please specify the file path. & pause & exit

set THIS_DIRECTORY_PATH=%~dp0
set PROCESSING_TYPE=%~n0
set IMAGE_FILE_PATH=%~1

set /P HORIZONTAL_COLOR="Input the source horizontal color: "
set /P VERTICAL_COLOR="Input the source vertical color: "
set /P BORDER_COLOR="Input the border color: "
set /P FRAME_COLOR="Input the frame color: "
set /P BACKGROUND_COLOR="Input the background color: "

PowerShell -File "%THIS_DIRECTORY_PATH%scripts\Do-Imaging.ps1" ^
  -ProcessingType "%PROCESSING_TYPE%" ^
  -SourceImageFilePath "%IMAGE_FILE_PATH%" ^
  -Option1 "%HORIZONTAL_COLOR%" ^
  -Option2 "%VERTICAL_COLOR%" ^
  -Option3 "%BORDER_COLOR%" ^
  -Option4 "%FRAME_COLOR%" ^
  -Option5 "%BACKGROUND_COLOR%" ^
