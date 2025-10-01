# Fix UTF-8 encoding issues in GeoJSON file
$filePath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"

Write-Host "Reading file..." -ForegroundColor Yellow
$content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)

Write-Host "Fixing encoding issues..." -ForegroundColor Yellow

# Fix broken UTF-8 characters
$content = $content -replace [char]0xFFFD, 'e'  # Replace replacement character

# Common French character fixes
$patterns = @(
    @('Pi.{1,2}zom.{1,2}tre', 'Piezometre'),
    @('Rivi.{1,2}re', 'Riviere'),
    @('d.{1,2}partement', 'departement'),
    @('pr.{1,2}l.{1,2}vement', 'prelevement'),
    @('qualit.{1,2}', 'qualite'),
    @('g.{1,2}n.{1,2}ral', 'general'),
    @('cr.{1,2}.{1,2}', 'cree'),
    @('D.{1,2}bit', 'Debit'),
    @('Op.{1,2}ration', 'Operation'),
    @('l.{1,2}s-', 'les-'),
    @('for.{1,2}t', 'foret')
)

foreach ($pattern in $patterns) {
    $content = $content -replace $pattern[0], $pattern[1]
}

# Then add proper French accents
$content = $content -replace 'Piezometre', 'Piézomètre'
$content = $content -replace 'Piezometres', 'Piézomètres'
$content = $content -replace 'Riviere', 'Rivière'
$content = $content -replace 'Rivieres', 'Rivières'
$content = $content -replace 'departement', 'département'
$content = $content -replace 'prelevement', 'prélèvement'
$content = $content -replace 'qualite', 'qualité'
$content = $content -replace 'general', 'général'
$content = $content -replace 'cree', 'créé'
$content = $content -replace 'Debit', 'Débit'
$content = $content -replace 'Operation', 'Opération'
$content = $content -replace '\bles-', 'lès-'
$content = $content -replace 'foret', 'forêt'

Write-Host "Writing fixed content back to file..." -ForegroundColor Yellow

# Write with UTF-8 encoding
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($filePath, $content, $utf8NoBom)

Write-Host "Encoding issues fixed successfully!" -ForegroundColor Green