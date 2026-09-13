# 构建基座 mod 并组装 mods/base_content 布局：manifest.json 的顶层声明 code/*.dll，
# display 段声明 code_display/*.dll 与 assets/*.pck 两类展示产物。
# 前置：$env:DCB_GODOT_4_7_1 指向 Godot_v4.7.1-stable_mono_win64.exe（mono 版，与展示工程 Godot.NET.Sdk 同版本）。
# 在解决方案根执行：powershell -File pack-mod.ps1
$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$modId = 'base_content'
$godotVersion = '4.7.1'
$dataProject = Join-Path $repoRoot 'DungeonChessBattle.BaseContent\DungeonChessBattle.BaseContent.csproj'
$dataDll = Join-Path $repoRoot 'DungeonChessBattle.BaseContent\bin\Release\net10.0\DungeonChessBattle.BaseContent.dll'
$displayProject = Join-Path $repoRoot 'DungeonChessBattle.BaseContent.Display\DungeonChessBattle.BaseContent.Display.csproj'
$assetsProject = Join-Path $repoRoot 'DungeonChessBattle.BaseContent.Display'
$displayDll = Join-Path $assetsProject '.godot\mono\temp\bin\Release\DungeonChessBattle.BaseContent.Display.dll'
$modDir = Join-Path $repoRoot "mods\$modId"
# export_presets.cfg 内的预设名：--export-pack 只按它取资源导出规则，不产可执行文件
$exportPreset = 'Windows Desktop'

# 版本声明唯一来源：程序集版本与进指纹的 manifest.version 不得各写一份
$sdkVersion = [regex]::Match((Get-Content (Join-Path $repoRoot 'Directory.Build.props') -Raw),
    '<Version>([^<]+)</Version>').Groups[1].Value
if (-not $sdkVersion) { throw 'Directory.Build.props 未声明 <Version>' }

# CLI 只经版本化环境变量定位，用 PE 版本资源自证：GUI 版 exe 非 headless 时直写控制台不走管道，--version 捕不到
$godotEnv = 'DCB_GODOT_' + ($godotVersion -replace '\.', '_')
$godot = [Environment]::GetEnvironmentVariable($godotEnv)
if (-not $godot) { throw "未设置 $godotEnv，其值指向 Godot_v$godotVersion-stable_mono_win64.exe" }
if (-not (Test-Path $godot -PathType Leaf)) { throw "$godotEnv 指向的文件不存在：$godot" }
$godotVer = (Get-Item $godot).VersionInfo
if ($godotVer.FileVersion -ne $godotVersion) {
    throw "$godotEnv 指向的 CLI 版本为 $($godotVer.FileVersion)，需 $godotVersion"
}
if ($godotVer.ProductVersion -notmatch 'mono') { throw "$godotEnv 指向的 CLI 非 mono 版，导出 C# 工程必失败" }

# 环境校验通过后才重建产物：配错环境不该把上一次的可用产物一起清掉
Remove-Item $modDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path (Join-Path $modDir 'code'),
    (Join-Path $modDir 'code_display'), (Join-Path $modDir 'assets') | Out-Null

# 数据代码：服务端装载，code/*.dll
dotnet build $dataProject -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Copy-Item $dataDll (Join-Path $modDir 'code') -Force

# 展示代码：仅客户端装载，code_display/*.dll（Godot.NET.Sdk 输出在展示工程的 .godot/mono/temp/bin/Release）
dotnet build $displayProject -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if (-not (Test-Path $displayDll)) { throw "展示 DLL 未构建：$displayDll" }
Copy-Item $displayDll (Join-Path $modDir 'code_display') -Force

# 展示资源：展示工程内 mods/{id}/assets 整体导出 PCK，包内资源即 res://mods/{id}/ 前缀寻址，不进内容指纹。
# --export-pack 隐含 --import；输出路径给绝对路径——相对路径按 project.godot 所在目录解析而非当前目录。
# Godot 主程序是 GUI 子系统 exe，& 调用不等待其退出，必须 Start-Process -Wait 才拿得到真实退出码与产物
$pckFile = Join-Path $modDir 'assets\base_content.pck'
$godotArgs = @('--headless', '--path', "`"$assetsProject`"",
    '--export-pack', "`"$exportPreset`"", "`"$pckFile`"") -join ' '
$export = Start-Process -FilePath $godot -ArgumentList $godotArgs -NoNewWindow -Wait -PassThru
if ($export.ExitCode -ne 0) { exit $export.ExitCode }
if (-not (Test-Path $pckFile)) { throw "PCK 导出失败：$pckFile 不存在" }

$manifest = [ordered]@{
    id           = $modId
    version      = $sdkVersion
    revision     = '0'
    dependencies = @()
    # 顶层是数据面声明，display 是展示面声明：两段各由自己的消费方读取裁决
    # 入口与资源包显式声明；入口 DLL 所在目录自动登记为依赖探测根，依赖与入口同目录时无需再声明
    code         = @("code/$(Split-Path -Leaf $dataDll)")
    display      = [ordered]@{
        code  = @("code_display/$(Split-Path -Leaf $displayDll)")
        packs = @("assets/$(Split-Path -Leaf $pckFile)")
    }
}
# 无 BOM 写出：装载侧按 UTF-8 读 manifest
[System.IO.File]::WriteAllText((Join-Path $modDir 'manifest.json'), ($manifest | ConvertTo-Json -Depth 4))

Write-Host "mod 已组装：$modDir"
Write-Host "  code/*.dll         数据代码（服务端装载，进指纹）"
Write-Host "  code_display/*.dll 展示代码（仅客户端装载，不进指纹）"
Write-Host "  assets/*.pck       展示资源包（仅客户端装载，不进指纹）"
Write-Host "把整个 $modId 目录拷进游戏 user://mods 下，重启进程生效"