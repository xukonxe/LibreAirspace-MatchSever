<#
.SYNOPSIS
用于自动化脚本的一系列基础设施/实用程序！
#>
# 类似C语言的防止头文件多次Include的机制（
if ($_SCRIPT_COMMON_INCLUDED) {
	exit 0
}
$_SCRIPT_COMMON_INCLUDED	= $true

# 脚本输出信息时，信息的前缀
$_BUILD_SCRIPT_MESG_PREFIX	= "Build script: "
$_SOURCE_DIRECTORY_NAME		= "src"
$_ARTIFACT_DIRECTORY_NAME	= "artifact"
$_ProjectCsprojFile			= (Get-Item -Path (Join-Path $_SOURCE_DIRECTORY_NAME "战雷革命游戏服务器.csproj") 2> $null <# 避免cmdlet输出信息污染终端 #>)
