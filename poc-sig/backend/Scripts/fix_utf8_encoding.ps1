# Fix UTF-8 encoding issues in GeoJSON files
# This handles the common mojibake pattern where UTF-8 was misinterpreted as Latin-1

$inputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet_fixed.geojson"
$outputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet_utf8.geojson"

Write-Host "Fixing UTF-8 encoding issues..." -ForegroundColor Cyan

# Read the file as raw bytes to preserve the original encoding
$bytes = [System.IO.File]::ReadAllBytes($inputFile)

# Convert from Latin-1 (which is how the UTF-8 bytes were misinterpreted)
$latin1 = [System.Text.Encoding]::GetEncoding('ISO-8859-1')
$text = $latin1.GetString($bytes)

# Common mojibake replacements for French characters
$replacements = @{
    'Ã©' = 'é'
    'Ã¨' = 'è'
    'Ãª' = 'ê'
    'Ã ' = 'à'
    'Ã¢' = 'â'
    'Ã´' = 'ô'
    'Ã»' = 'û'
    'Ã§' = 'ç'
    'Ã®' = 'î'
    'Ã¯' = 'ï'
    'Ã‰' = 'É'
    'Ãˆ' = 'È'
    'ÃŠ' = 'Ê'
    'Ã€' = 'À'
    'Ã‚' = 'Â'
    'Ã"' = 'Ô'
    'Ã›' = 'Û'
    'Ã‡' = 'Ç'
    'ÃŽ' = 'Î'
    'Ã' = 'Ï'
}

foreach ($key in $replacements.Keys) {
    $text = $text.Replace($key, $replacements[$key])
}

# Save as proper UTF-8
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($outputFile, $text, $utf8NoBom)

Write-Host "✓ Fixed encoding and saved to: $outputFile" -ForegroundColor Green

# Also copy to runtime directory
$runtimePath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"
[System.IO.File]::Copy($outputFile, $runtimePath, $true)

Write-Host "✓ Copied to runtime directory" -ForegroundColor Green

# Verify the fix
$verifyText = [System.IO.File]::ReadAllText($outputFile, $utf8NoBom)
if ($verifyText.Contains("Piézomètre") -and $verifyText.Contains("Rivières") -and $verifyText.Contains("épuration")) {
    Write-Host "✅ UTF-8 encoding successfully fixed!" -ForegroundColor Green
    Write-Host "Verified French characters: Piézomètre, Rivières, épuration" -ForegroundColor Yellow
} else {
    Write-Host "⚠ Some encoding issues may remain" -ForegroundColor Yellow
}