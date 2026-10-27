param([string]$PreviewPath)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$temporary=Join-Path ([IO.Path]::GetTempPath()) ('CursorStudio-Icon-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
$generator=Join-Path $temporary 'GenerateIcon.exe'
try {
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    & $compiler /nologo /target:winexe /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.Linq.dll "/out:$generator" "$PSScriptRoot\GenerateIcon.cs"
    if($LASTEXITCODE -ne 0){throw '图标生成器编译失败'}
    $arguments='"'+$project+'\Source\AppIcon.svg" "'+$project+'\Source\App.ico"'
    if($PreviewPath){$arguments+=' "'+[IO.Path]::GetFullPath($PreviewPath)+'"'}
    $process=Start-Process -FilePath $generator -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
    if($process.ExitCode -ne 0){throw '图标生成失败'}
} finally {
    if(Test-Path -LiteralPath $generator){Remove-Item -LiteralPath $generator -Force}
    Remove-Item -LiteralPath $temporary
}
