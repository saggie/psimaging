param (
    [Parameter(Mandatory = $true)]
    [string]
    $ProcessingType,

    [Parameter(Mandatory = $true)]
    [string]
    $SourceImageFilePath,

    [Parameter(Mandatory = $false)]
    [PSObject]
    $Option1
)

# load C# files
$thisDirectoryPath = Split-Path $MyInvocation.MyCommand.Path -Parent
Add-Type -Path (Join-Path $thisDirectoryPath 'ImagingCore.cs'), `
               (Join-Path $thisDirectoryPath 'ImageProcesser.cs') `
         -ReferencedAssemblies System.Drawing

# resolve file path
$resolvedFilePath = Resolve-Path $SourceImageFilePath

# get image information
Add-Type -AssemblyName System.Drawing
$sourceImage = [System.Drawing.Image]::FromFile($resolvedFilePath)

# convert image to pixels
$sourcePixels = [PSImaging.BitmapConverter]::ConvertBitmapToPixels($sourceImage)
if ($sourceImage -ne $null)
{
    $sourceImage.Dispose()
}

# process image
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
    default { exit 0 }
}
$newPixels = $imageProcesser.Process($sourcePixels)

# deconvert image
$outputImage = [PSImaging.BitmapConverter]::ConvertPixelsToBitmap($newPixels)

# save processed image
$outputFileLocation = Split-Path $resolvedFilePath -Parent
$timestamp = (Get-Date).ToString("_yyyyMMdd_HHmmss")
$outputFileBaseName = (Get-ChildItem $resolvedFilePath).BaseName + $timestamp
$outputFileExtention = (Get-ChildItem $resolvedFilePath).Extension
$outputFileFullName = $outputFileBaseName + $outputFileExtention
$outputFilePath = Join-Path $outputFileLocation $outputFileFullName
$outputImageFormat = [PSImaging.ImageFormatResolver]::ResolveFromExtension($outputFileExtention)
$outputImage.Save($outputFilePath, $outputImageFormat)
