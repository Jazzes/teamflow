# Выгружает конфигурацию файловой базы 1С в XML-исходники (для коммита в Git).
# Пример: .\tools\Dump-Base.ps1 -Base C:\lab5\bases\dev2
param(
    [Parameter(Mandatory = $true)][string]$Base,
    [string]$Src = '',
    [string]$Platform = 'C:\Program Files (x86)\1cv8t\8.3.27.1508\bin\1cv8t.exe'
)
$ErrorActionPreference = 'Stop'
if (-not $Src) { $Src = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..\src' }
$log = Join-Path $env:TEMP 'lab5-dump.log'
$target = (Resolve-Path $Src).Path
Get-ChildItem $target | Remove-Item -Recurse -Force
$p = Start-Process -FilePath $Platform -ArgumentList @('DESIGNER', '/F', "`"$Base`"", '/DumpConfigToFiles', "`"$target`"", '/Out', "`"$log`"", '/DisableStartupDialogs') -Wait -PassThru -WindowStyle Hidden
if ($p.ExitCode -ne 0) { throw "1С завершилась с кодом $($p.ExitCode): $(Get-Content $log -Encoding UTF8)" }
Write-Output "Конфигурация $Base выгружена в $target"