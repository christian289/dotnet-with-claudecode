#!/bin/bash
# Git hooks 설치 스크립트
# Git hooks installation script

REPO_ROOT=$(git rev-parse --show-toplevel)

# Set custom hooks path
# 커스텀 hooks 경로 설정
git config core.hooksPath "$REPO_ROOT/.githooks"

echo "Git hooks installed successfully."
echo "Hooks path set to: .githooks"
