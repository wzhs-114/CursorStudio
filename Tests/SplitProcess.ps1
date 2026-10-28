param([string]$Output)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
if(-not $Output){$Output=Join-Path $project 'Tests\split-process-qa\result.json'}
$Output=[IO.Path]::GetFullPath($Output)
New-Item -ItemType Directory -Path (Split-Path -Parent $Output) -Force | Out-Null
& "$project\Source\Build.ps1"
$files=@(Get-ChildItem -LiteralPath "$project\Source" -Filter '*.cs' | ForEach-Object FullName)+@((Join-Path $PSScriptRoot 'SplitProcessQA.cs'))
$qa=Join-Path (Split-Path -Parent $Output) 'SplitProcessQA.exe'
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /main:SplitProcessQA /optimize+ /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Core.dll /reference:Microsoft.CSharp.dll "/out:$qa" $files
if($LASTEXITCODE -ne 0){throw 'QA build failed'}
$arguments='"'+(Join-Path $project 'dist\CursorStudio\CursorStudio.exe')+'" "'+$Output+'"'
$process=Start-Process -FilePath $qa -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
Get-Content -LiteralPath $Output
if($process.ExitCode -ne 0){throw 'Split-process integration test failed'}
