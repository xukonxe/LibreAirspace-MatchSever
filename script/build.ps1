<#
.SYNOPSIS
用于构建自由空域房间列表服务器的PowerShell脚本！
#>
param (
	[switch]
	$ClearArtifactDirectory,
	[bool]
	$BuildSelfContained,
	[bool]
	$BuildDebug
	<# Naidesu! #>
)

. (Join-Path (Get-Item $PSCommandPath).DirectoryName "script_common.ps1")
if ($null -eq $_ProjectCsprojFile) {
	Write-Error "${_BUILD_SCRIPT_MESG_PREFIX}应该将命令解释器的当前目录设置成项目的根目录后再执行脚本，否则脚本无法定位文件。
且可能会出现未定义的行为。"
	exit 1
}

# 检查一下"工件"目录的状态
$_ArtifactDirectory = (Get-Item -Path $_ARTIFACT_DIRECTORY_NAME 2> $null)
# 如果没有这个目录则创建目录，如果有同名文件则报错。
if ($null -eq $_ArtifactDirectory) {
	New-Item -Path $_ARTIFACT_DIRECTORY_NAME -ItemType Directory
} elseif ($_ArtifactDirectory -isnot [System.IO.DirectoryInfo]) {
	Write-Error "${_BUILD_SCRIPT_MESG_PREFIX}为什么会有一个名为${_ARTIFACT_DIRECTORY_NAME}的文件？此脚本默认此文件名的文件应该是目录。
请手动删除此文件。"
	exit 1
}
if ($ClearArtifactDirectory) {
	Get-ChildItem $_ARTIFACT_DIRECTORY_NAME | Remove-Item -Recurse -Verbose
}

# 构建起来构建程序的命令！
[string] $_BuildTheProgram_Cmd = "dotnet publish $_ProjectCsprojFile --artifacts-path $_ARTIFACT_DIRECTORY_NAME"
$_BuildTheProgram_Cmd += $PublishSelfContained ? " --self-contained" : " --no-self-contained"
$_BuildTheProgram_Cmd += " --configuration $($BuildDebug ? "Debug" : "Release")"
# 构建程序！
Invoke-Expression -Command $_BuildTheProgram_Cmd
