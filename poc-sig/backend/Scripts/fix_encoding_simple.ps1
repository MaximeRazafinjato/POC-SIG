# Fix UTF-8 encoding issues in GeoJSON file
$inputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet_fixed.geojson"
$outputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"
$runtimePath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"

Write-Host "Reading file..." -ForegroundColor Yellow
$content = [System.IO.File]::ReadAllText($inputFile, [System.Text.Encoding]::UTF8)

Write-Host "Fixing encoding issues..." -ForegroundColor Yellow

# Fix mojibake patterns (UTF-8 misinterpreted as Latin-1)
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
}

foreach ($old in $replacements.Keys) {
    $new = $replacements[$old]
    $content = $content.Replace($old, $new)
}

# Additional specific replacements
$content = $content.Replace('PiÃ©zomÃ¨tre', 'Piézomètre')
$content = $content.Replace('RiviÃ¨res', 'Rivières')
$content = $content.Replace('GÃ©rardmer', 'Gérardmer')
$content = $content.Replace('Pierre-PercÃ©e', 'Pierre-Percée')
$content = $content.Replace('Ã©puration', 'épuration')

Write-Host "Writing fixed content to files..." -ForegroundColor Yellow

# Write with UTF-8 encoding (no BOM)
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($outputFile, $content, $utf8NoBom)

# Copy to runtime directory
[System.IO.File]::Copy($outputFile, $runtimePath, $true)

Write-Host "Files saved to:" -ForegroundColor Green
Write-Host "  - $outputFile" -ForegroundColor Cyan
Write-Host "  - $runtimePath" -ForegroundColor Cyan

# Verify the fix
if ($content.Contains("Piézomètre") -and $content.Contains("Rivières") -and $content.Contains("épuration")) {
    Write-Host "UTF-8 encoding successfully fixed!" -ForegroundColor Green
    Write-Host "Verified: Piézomètre, Rivières, épuration" -ForegroundColor Yellow
} else {
    Write-Host "Some encoding issues may remain" -ForegroundColor Yellow
}