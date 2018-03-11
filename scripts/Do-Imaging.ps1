param (
    [Parameter(Mandatory = $true)]
    [string]
    $ProcessingType,

    [Parameter(Mandatory = $true)]
    [string]
    $SourceImageFilePath,

    [Parameter(Mandatory = $false)]
    [PSObject]
    $Option1,

    [Parameter(Mandatory = $false)]
    [PSObject]
    $Option2,

    [Parameter(Mandatory = $false)]
    [PSObject]
    $Option3
)

# Load C# files
$thisDirectoryPath = Split-Path $MyInvocation.MyCommand.Path -Parent
Add-Type -Path (Join-Path $thisDirectoryPath 'ImagingCore.cs'), `
               (Join-Path $thisDirectoryPath 'ImageProcesser.cs') `
         -ReferencedAssemblies System.Drawing

# Resolve the file path
$resolvedFilePath = Resolve-Path $SourceImageFilePath

# Load an image from the file
Add-Type -AssemblyName System.Drawing
$sourceImage = [System.Drawing.Image]::FromFile($resolvedFilePath)

# Convert image to pixels
$sourcePixels = [PSImaging.BitmapConverter]::ConvertBitmapToPixels($sourceImage)
if ($sourceImage -ne $null)
{
    $sourceImage.Dispose()
}

# Process image
switch ($ProcessingType)
{
    'ConvertTo-Grayscale' {
        $imageProcesser = New-Object PSImaging.GrayScaleConverter
    }
    'Add-Frame' {
        $imageProcesser = New-Object PSImaging.FrameAdder
        $imageProcesser.borderSize = [Int32]$Option1
    }
    'Apply-MedianFilter' {
        $imageProcesser = New-Object PSImaging.MedianFilter
        $imageProcesser.distance = [Int32]$Option1
    }
    'Draw-Edge' {
        $imageProcesser = New-Object PSImaging.EdgeDrawer
        $imageProcesser.SetLevel([Int32]$Option1)
        $imageProcesser.SetDrawingColor([string]$Option2)
        $imageProcesser.SetBackGroundColor([string]$Option3)
    }
    'Replace-Color' {
        $imageProcesser = New-Object PSImaging.ColorReplacer
        $imageProcesser.SetColorsToReplace([string]$Option1, [string]$Option2)
    }
    'Cleanup-Color' {
        $imageProcesser = New-Object PSImaging.ColorCleaner
        $imageProcesser.SetAllowedColor([string]$Option1)
        $imageProcesser.SetResultColor([string]$Option2)
    }
    default { exit 0 }
}
$newPixels = $imageProcesser.Process($sourcePixels)

# Deconvert image
$outputImage = [PSImaging.BitmapConverter]::ConvertPixelsToBitmap($newPixels)

# Save processed image
$outputFileLocation = Split-Path $resolvedFilePath -Parent
$timestamp = (Get-Date).ToString("_yyyyMMdd_HHmmss")
$outputFileBaseName = (Get-ChildItem $resolvedFilePath).BaseName + $timestamp
$outputFileExtention = (Get-ChildItem $resolvedFilePath).Extension
$outputFileFullName = $outputFileBaseName + $outputFileExtention
$outputFilePath = Join-Path $outputFileLocation $outputFileFullName
$outputImageFormat = [PSImaging.ImageFormatResolver]::ResolveFromExtension($outputFileExtention)
$outputImage.Save($outputFilePath, $outputImageFormat)

return $outputFilePath
