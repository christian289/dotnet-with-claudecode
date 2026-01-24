#!/usr/bin/env pwsh
# Git hooks 설치 스크립트
# Git hooks installation script

$RepoRoot = git rev-parse --show-toplevel

# Set custom hooks path
# 커스텀 hooks 경로 설정
git config core.hooksPath "$RepoRoot/.githooks"

Write-Host "Git hooks installed successfully." -ForegroundColor Green
Write-Host "Hooks path set to: .githooks" -ForegroundColor Cyan
