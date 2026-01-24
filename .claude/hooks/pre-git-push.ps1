#!/usr/bin/env pwsh
# pre-git-push.ps1
# Hook wrapper that checks if command is git push and runs version bump
# git push 명령인지 확인하고 버전 업데이트를 실행하는 hook wrapper

# Read input from stdin (Claude Code passes JSON context)
# stdin에서 입력 읽기 (Claude Code가 JSON context를 전달)
$input = [Console]::In.ReadToEnd()

try {
    $context = $input | ConvertFrom-Json
    $command = $context.tool_input.command

    # Check if this is a git push command
    # git push 명령인지 확인
    if ($command -match "^git\s+push") {
        # Check for explicit skip indicators
        # 명시적 skip 지시자 확인
        if ($env:SKIP_VERSION_BUMP -eq "true") {
            Write-Host "Version bump skipped by environment variable"
            exit 0
        }

        # Run the version bump script
        # 버전 업데이트 스크립트 실행
        $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
        $bumpScript = Join-Path $scriptDir "bump-wpf-dev-pack-version.ps1"

        if (Test-Path $bumpScript) {
            & $bumpScript
        }
    }
}
catch {
    # If JSON parsing fails, just continue (non-blocking)
    # JSON 파싱 실패 시 계속 진행 (non-blocking)
    Write-Host "Hook parse error (non-fatal): $_"
}

exit 0
