# Fix UTF-8 encoding issues in GeoJSON file

$filePath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"

Write-Host "Reading file..." -ForegroundColor Yellow
$content = Get-Content $filePath -Raw -Encoding UTF8

Write-Host "Fixing encoding issues..." -ForegroundColor Yellow

# Fix common encoding issues
$replacements = @{
    'Pi�zom�tre' = 'Piézomètre'
    'Pi�zom�tres' = 'Piézomètres'
    'Rivi�re' = 'Rivière'
    'Rivi�res' = 'Rivières'
    'd�partement' = 'département'
    'pr�l�vement' = 'prélèvement'
    'qualit�' = 'qualité'
    'g�n�ral' = 'général'
    'cr��' = 'créé'
    'D�bit' = 'Débit'
    'Op�ration' = 'Opération'
    'l��s' = 'lès'
    'for�t' = 'forêt'
    'Qualit�' = 'Qualité'
    'G�n�ral' = 'Général'
    'donn�es' = 'données'
    'temp�rature' = 'température'
    'r�seau' = 'réseau'
    '�tat' = 'état'
    '�volution' = 'évolution'
    'activit�' = 'activité'
    'capacit�' = 'capacité'
    'mesur�' = 'mesuré'
    'observ�' = 'observé'
    'derni�re' = 'dernière'
    'ann�e' = 'année'
    'p�riode' = 'période'
    '�quipement' = 'équipement'
    '�chantillon' = 'échantillon'
    'r�sultat' = 'résultat'
    'acc�s' = 'accès'
    'coordonn�es' = 'coordonnées'
    'syst�me' = 'système'
}

foreach ($key in $replacements.Keys) {
    $content = $content -replace [regex]::Escape($key), $replacements[$key]
}

# Also fix any remaining common patterns
$content = $content -replace '([a-zA-Z])�([a-zA-Z])', '$1é$2'
$content = $content -replace '([a-zA-Z])�([a-zA-Z])', '$1è$2'
$content = $content -replace '([a-zA-Z])�([a-zA-Z])', '$1à$2'

Write-Host "Writing fixed content back to file..." -ForegroundColor Yellow

# Write with UTF-8 encoding without BOM
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($filePath, $content, $utf8NoBom)

Write-Host "Encoding issues fixed successfully!" -ForegroundColor Green

# Verify the fix
Write-Host "`nVerifying fixes..." -ForegroundColor Cyan
$verifyContent = Get-Content $filePath -Raw -Encoding UTF8
if ($verifyContent -match 'Piézomètre') {
    Write-Host "✓ 'Piézomètre' correctly encoded" -ForegroundColor Green
}
if ($verifyContent -match 'Rivière') {
    Write-Host "✓ 'Rivière' correctly encoded" -ForegroundColor Green
}
if ($verifyContent -notmatch 'Pi�zom�tre') {
    Write-Host "✓ No encoding issues found for 'Piézomètre'" -ForegroundColor Green
} else {
    Write-Host "✗ Still has encoding issues" -ForegroundColor Red
}