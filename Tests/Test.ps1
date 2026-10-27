param([switch]$Integration)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
& "$project\Source\Build.ps1"
$result=Join-Path ([IO.Path]::GetTempPath()) ('CursorStudio-Test-'+[Guid]::NewGuid().ToString('N')+'.json')
$mode=if($Integration){'--integration-test'}else{'--self-test'}
$process=Start-Process -FilePath "$project\dist\CursorStudio\CursorStudio.exe" -ArgumentList ($mode+' "'+$result+'"') -WindowStyle Hidden -Wait -PassThru
if(Test-Path -LiteralPath $result){Get-Content -LiteralPath $result}
if($process.ExitCode -ne 0){throw "验证失败；报告：$result"}
