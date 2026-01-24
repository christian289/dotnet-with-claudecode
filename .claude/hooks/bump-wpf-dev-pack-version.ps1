#!/usr/bin/env pwsh
# bump-wpf-dev-pack-version.ps1
# Automatically bumps wpf-dev-pack patch version before git push
# 자동으로 wpf-dev-pack patch 버전을 git push 전에 올립니다

param(
    [string]$RepoRoot = (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
)

$WpfDevPackPath = Join-Path $RepoRoot "wpf-dev-pack"
$PluginJsonPath = Join-Path $WpfDevPackPath ".claude-plugin/plugin.json"
$ReadmePath = Join-Path $WpfDevPackPath "README.md"

# Check if wpf-dev-pack exists
# wpf-dev-pack 존재 여부 확인
if (-not (Test-Path $PluginJsonPath)) {
    Write-Host "wpf-dev-pack not found, skipping version bump"
    exit 0
}

# Check if there are wpf-dev-pack changes to push
# wpf-dev-pack 변경사항이 있는지 확인
$changedFiles = git -C $RepoRoot diff --name-only origin/main...HEAD 2>$null
if (-not $changedFiles) {
    $changedFiles = git -C $RepoRoot diff --name-only HEAD~1 2>$null
}

$wpfChanges = $changedFiles | Where-Object { $_ -like "wpf-dev-pack/*" -and $_ -notlike "*plugin.json" -and $_ -notlike "*README.md" }

if (-not $wpfChanges) {
    Write-Host "No wpf-dev-pack content changes detected, skipping version bump"
    exit 0
}

# Check for [skip-version] in recent commit messages
# 최근 커밋 메시지에서 [skip-version] 확인
$recentCommits = git -C $RepoRoot log --oneline -5 2>$null
if ($recentCommits -match "\[skip-version\]") {
    Write-Host "[skip-version] found in recent commits, skipping version bump"
    exit 0
}

# Read current version from plugin.json
# plugin.json에서 현재 버전 읽기
$pluginJson = Get-Content $PluginJsonPath -Raw | ConvertFrom-Json
$currentVersion = $pluginJson.version

if (-not $currentVersion) {
    Write-Host "Could not read version from plugin.json"
    exit 1
}

# Parse version
# 버전 파싱
$versionParts = $currentVersion -split '\.'
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$patch = [int]$versionParts[2]

# Bump patch version
# patch 버전 올리기
$newPatch = $patch + 1
$newVersion = "$major.$minor.$newPatch"

Write-Host "Bumping wpf-dev-pack version: $currentVersion -> $newVersion"

# Update plugin.json
# plugin.json 업데이트
$pluginJson.version = $newVersion
$pluginJson | ConvertTo-Json -Depth 10 | Set-Content $PluginJsonPath -Encoding UTF8

# Update README.md badge
# README.md 배지 업데이트
$readmeContent = Get-Content $ReadmePath -Raw
$readmeContent = $readmeContent -replace "version-$currentVersion-blue", "version-$newVersion-blue"
$readmeContent | Set-Content $ReadmePath -Encoding UTF8 -NoNewline

# Stage and commit the version bump
# 버전 업데이트 스테이징 및 커밋
git -C $RepoRoot add $PluginJsonPath $ReadmePath
git -C $RepoRoot commit -m "chore(wpf-dev-pack): bump version to $newVersion"

Write-Host "Version bumped to $newVersion and committed"
exit 0
