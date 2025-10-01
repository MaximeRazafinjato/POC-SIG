# Script pour corriger définitivement l'encodage UTF-8

$scriptPath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"
$binPath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"

Write-Host "Correction définitive de l'encodage UTF-8..." -ForegroundColor Cyan

# Lire le fichier avec encodage par défaut (probablement Windows-1252)
$content = [System.IO.File]::ReadAllText($scriptPath, [System.Text.Encoding]::Default)

# Dictionnaire de remplacement des caractères mal encodés
$replacements = @{
    'PiÃ©zomÃ¨tre' = 'Piézomètre'
    'PiÃ©zomÃ¨tres' = 'Piézomètres'
    'dÃ©partement' = 'département'
    'lÃ¨s' = 'lès'
    'RiviÃ¨re' = 'Rivière'
    'RiviÃ¨res' = 'Rivières'
    'qualitÃ©' = 'qualité'
    'gÃ©nÃ©ral' = 'général'
    'crÃ©Ã©' = 'créé'
    'DÃ©bit' = 'Débit'
    'OpÃ©ration' = 'Opération'
    'forÃªt' = 'forêt'
    'prÃ©lÃ¨vement' = 'prélèvement'
    'donnÃ©es' = 'données'
    'tempÃ©rature' = 'température'
    'rÃ©seau' = 'réseau'
    'Ã©tat' = 'état'
    'Ã©volution' = 'évolution'
    'activitÃ©' = 'activité'
    'capacitÃ©' = 'capacité'
    'mesurÃ©' = 'mesuré'
    'observÃ©' = 'observé'
    'derniÃ¨re' = 'dernière'
    'annÃ©e' = 'année'
    'pÃ©riode' = 'période'
    'Ã©quipement' = 'équipement'
    'Ã©chantillon' = 'échantillon'
    'rÃ©sultat' = 'résultat'
    'accÃ¨s' = 'accès'
    'coordonnÃ©es' = 'coordonnées'
    'systÃ¨me' = 'système'
    # Caractères isolés
    'Ã©' = 'é'
    'Ã¨' = 'è'
    'Ãª' = 'ê'
    'Ã ' = 'à'
    'Ã¢' = 'â'
    'Ã®' = 'î'
    'Ã´' = 'ô'
    'Ã»' = 'û'
    'Ã§' = 'ç'
}

# Appliquer tous les remplacements
foreach ($oldText in $replacements.Keys) {
    $newText = $replacements[$oldText]
    $content = $content -replace [regex]::Escape($oldText), $newText
}

# Écrire le fichier avec encodage UTF-8 sans BOM
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

# Sauvegarder dans les deux emplacements
[System.IO.File]::WriteAllText($scriptPath, $content, $utf8NoBom)
Write-Host "✓ Fichier source corrigé: $scriptPath" -ForegroundColor Green

[System.IO.File]::WriteAllText($binPath, $content, $utf8NoBom)
Write-Host "✓ Fichier runtime corrigé: $binPath" -ForegroundColor Green

# Vérification
$verifyContent = [System.IO.File]::ReadAllText($binPath, [System.Text.Encoding]::UTF8)
$hasIssues = $false

if ($verifyContent -match 'PiÃ©zomÃ¨tre') {
    Write-Host "✗ Problème d'encodage détecté encore présent" -ForegroundColor Red
    $hasIssues = $true
} else {
    Write-Host "✓ Plus de 'PiÃ©zomÃ¨tre' mal encodé" -ForegroundColor Green
}

if ($verifyContent -match 'Piézomètre') {
    Write-Host "✓ 'Piézomètre' correctement encodé trouvé" -ForegroundColor Green
} else {
    Write-Host "⚠ 'Piézomètre' non trouvé dans le fichier" -ForegroundColor Yellow
}

if (-not $hasIssues) {
    Write-Host "`n✅ Encodage corrigé avec succès!" -ForegroundColor Green
    Write-Host "Veuillez maintenant:" -ForegroundColor Yellow
    Write-Host "1. Nettoyer la base de données (http://localhost:5050/api/admin/clean-database)" -ForegroundColor Yellow
    Write-Host "2. Recharger les données (http://localhost:5050/api/admin/load-demo-data)" -ForegroundColor Yellow
} else {
    Write-Host "`n⚠ Des problèmes d'encodage persistent. Vérifiez le fichier manuellement." -ForegroundColor Red
}