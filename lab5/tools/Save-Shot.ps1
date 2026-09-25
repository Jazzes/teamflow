# Сохраняет снимок окна (по части заголовка) в PNG. Окно захватывается целиком, даже если его что-то перекрывает.
# Пример: .\tools\Save-Shot.ps1 -Title 'Конфигуратор' -Path ..\screenshots\01.png
param(
    [Parameter(Mandatory = $true)][string]$Title,
    [Parameter(Mandatory = $true)][string]$Path
)
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win32Shot {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdc, uint flags);
}
"@
[Win32Shot]::SetProcessDPIAware() | Out-Null
$proc = Get-Process | Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle -like "*$Title*" } | Select-Object -First 1
if (-not $proc) { throw "Окно с заголовком '$Title' не найдено" }
$rect = New-Object Win32Shot+RECT
[Win32Shot]::GetWindowRect($proc.MainWindowHandle, [ref]$rect) | Out-Null
$w = $rect.Right - $rect.Left; $h = $rect.Bottom - $rect.Top
$bmp = New-Object System.Drawing.Bitmap($w, $h)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
[Win32Shot]::PrintWindow($proc.MainWindowHandle, $hdc, 2) | Out-Null
$g.ReleaseHdc($hdc); $g.Dispose()
$dir = Split-Path -Parent $Path
if ($dir) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
$bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
Write-Output "Снимок '$($proc.MainWindowTitle)' ($w x $h) сохранён в $Path"