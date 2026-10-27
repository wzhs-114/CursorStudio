param([string]$Version='1.0.0')
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$exe=Join-Path $project 'dist\CursorStudio\CursorStudio.exe'
if(-not (Test-Path -LiteralPath $exe)){throw '请先运行 Source\Build.ps1'}
$staging=Join-Path $project ('dist\public\'+$Version+'\CursorStudio')
New-Item -ItemType Directory -Path $staging -Force | Out-Null
$files=@{
    'CursorStudio.exe'=$exe
    'README.md'=(Join-Path $project 'README.md')
    'LICENSE'=(Join-Path $project 'LICENSE')
    'CONTRIBUTORS.md'=(Join-Path $project 'CONTRIBUTORS.md')
    '恢复原指针.bat'=(Join-Path $PSScriptRoot 'Restore.bat')
}
foreach($name in $files.Keys){Copy-Item -LiteralPath $files[$name] -Destination (Join-Path $staging $name) -Force}
$contents=Get-ChildItem -LiteralPath $staging -Force
if($contents.Count -ne $files.Count -or @($contents | Where-Object { $_.PSIsContainer -or -not $files.ContainsKey($_.Name) }).Count){throw '公开包包含清单以外的文件，停止打包'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=Join-Path $project ('dist\CursorStudio-'+$Version+'-Windows11.zip')
if(Test-Path -LiteralPath $zip){Remove-Item -LiteralPath $zip -Force}
[IO.Compression.ZipFile]::CreateFromDirectory((Split-Path -Parent $staging),$zip,[IO.Compression.CompressionLevel]::Optimal,$false)
$archive=[IO.Compression.ZipFile]::OpenRead($zip)
try {
    if($archive.Entries.Count -ne $files.Count){throw 'ZIP 清单不匹配'}
    foreach($entry in $archive.Entries){if(-not $entry.FullName.StartsWith('CursorStudio/') -or -not $files.ContainsKey($entry.Name)){throw 'ZIP 含意外文件'}}
} finally {$archive.Dispose()}
Get-Item -LiteralPath $zip | Select-Object FullName,Length
Get-FileHash -LiteralPath $zip -Algorithm SHA256
