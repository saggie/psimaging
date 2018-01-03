@echo off

if "%~1" == "" echo Please specify the file path. & pause & exit

set THIS_DIRECTORY_PATH=%~dp0
set PROCESSING_TYPE=%~n0
set IMAGE_FILE_PATH=%~1

set /P ALLOWED_COLOR="Input the allowed color: "
set /P RESULT_COLOR="Input the result color: "

PowerShell -File "%THIS_DIRECTORY_PATH%scripts\Do-Imaging.ps1" ^
  -ProcessingType "%PROCESSING_TYPE%" ^
  -SourceImageFilePath "%IMAGE_FILE_PATH%" ^
  -Option1 "%ALLOWED_COLOR%" ^
  -Option2 "%RESULT_COLOR%"
