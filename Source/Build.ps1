$ErrorActionPreference='Stop'
$out=Join-Path (Split-Path -Parent $PSScriptRoot) 'dist\CursorStudio'
New-Item -ItemType Directory -Path $out -Force | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sources=Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object FullName
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Core.dll /reference:Microsoft.CSharp.dll "/win32icon:$PSScriptRoot\App.ico" "/resource:$PSScriptRoot\App.ico,CursorStudio.App.ico" "/win32manifest:$PSScriptRoot\app.manifest" "/out:$out\CursorStudio.exe" $sources
if($LASTEXITCODE -ne 0){throw '编译失败'}
