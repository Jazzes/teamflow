# Собирает файловую базу 1С из XML-исходников: загрузка конфигурации и обновление структуры БД.
# Пример: .\tools\Build-Base.ps1 -Base C:\lab5\bases\dev1
param(
    [Parameter(Mandatory = $true)][string]$Base,
    [string]$Src = '',
    [string]$Platform = 'C:\Program Files (x86)\1cv8t\8.3.27.1508\bin\1cv8t.exe'
)
$ErrorActionPreference = 'Stop'
if (-not $Src) { $Src = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..\src' }
$log = Join-Path $env:TEMP 'lab5-build.log'

function Invoke-1C([string[]]$Arguments) {
    $p = Start-Process -FilePath $Platform -ArgumentList $Arguments -Wait -PassThru -WindowStyle Hidden
    $text = Get-Content $log -Encoding UTF8 -ErrorAction SilentlyContinue
    if ($p.ExitCode -ne 0) { throw "1С завершилась с кодом $($p.ExitCode): $text" }
    return $text
}

if (-not (Test-Path (Join-Path $Base '1Cv8.1CD'))) {
    Invoke-1C @('CREATEINFOBASE', "File=`"$Base`"", '/Out', "`"$log`"", '/DisableStartupDialogs')
}
Invoke-1C @('DESIGNER', '/F', "`"$Base`"", '/LoadConfigFromFiles', "`"$((Resolve-Path $Src).Path)`"", '/UpdateDBCfg', '/Out', "`"$log`"", '/DisableStartupDialogs')
Write-Output "База $Base собрана из $Src"