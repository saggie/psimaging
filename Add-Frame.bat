@echo off

if "%~1" == "" echo Please specify the file path. & pause & exit

set THIS_DIRECTORY_PATH=%~dp0
set PROCESSING_TYPE=%~n0
set IMAGE_FILE_PATH=%~1

PowerShell -File "%THIS_DIRECTORY_PATH%scripts\Do-Imaging.ps1" -ProcessingType "%PROCESSING_TYPE%" -SourceImageFilePath "%IMAGE_FILE_PATH%" -Option1 1
