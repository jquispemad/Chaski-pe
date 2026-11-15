$ErrorActionPreference = 'Stop'
$repoChasqui = Split-Path -Parent $PSScriptRoot
$destinoFuentes = Join-Path $repoChasqui 'ChasquiUnity\Assets\Resources\EstiloInca'
if (-not $env:WINDIR) { throw 'Se necesita Windows con las fuentes Georgia instaladas.' }
$fuentesChasqui = @{
    'TextoTradicional.ttf' = 'georgia.ttf'
    'TituloTradicional.ttf' = 'georgiab.ttf'
}
foreach ($nombre in $fuentesChasqui.Keys) {
    $origenFuente = Join-Path $env:WINDIR ('Fonts\' + $fuentesChasqui[$nombre])
    if (-not (Test-Path -LiteralPath $origenFuente)) { throw "No se encontro la fuente local: $origenFuente" }
    Copy-Item -LiteralPath $origenFuente -Destination (Join-Path $destinoFuentes $nombre)
}
Write-Host 'Fuentes locales preparadas. Abra ChasquiUnity con Unity 6000.3.24f1.'
