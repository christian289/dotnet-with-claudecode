# GitHub Release 사용 규칙

## 핵심 원칙

- **`wpf-dev-pack`은 이 저장소의 주력 산출물입니다.** 저장소 메인 화면의
  "Releases" 사이드바 위젯에는 항상 `wpf-dev-pack`의 최신 릴리스가
  노출되어야 합니다.
- GitHub는 이 위젯에 표시할 릴리스를 **"Latest" 플래그**로 결정하며,
  기본값은 가장 최근에 게시된 pre-release/draft가 아닌 릴리스입니다.
  태그 이름이나 릴리스 제목으로 결정되는 것이 **아닙니다** — 태그
  접두사가 다르다고(예: `polylab3dstudio-v0.1.0` vs
  `wpf-dev-pack-v1.8.1`) `wpf-dev-pack`이 밀려나지 않는 것이 아닙니다.

## 규칙

- **`wpf-dev-pack` 이외의 모든 릴리스**(`samples/` 아래 샘플, `mcp/`
  서버를 NuGet이 아닌 GitHub Release로 배포하게 될 경우, 이 저장소의
  향후 다른 프로젝트 등)는 **반드시 `--prerelease`로 생성**합니다:

  ```bash
  gh release create <tag> <assets...> --prerelease \
    --title "..." --notes "..."
  ```

  Pre-release는 "Latest" 후보에서 제외되므로 `wpf-dev-pack`을 저장소
  메인 화면에서 밀어낼 수 없습니다.

- `wpf-dev-pack` 릴리스는 `/wpf-dev-pack-release` 스킬을 통해 평소처럼
  (`--prerelease` 없이) 생성하며, pre-release보다 항상 최신이므로 매번
  자동으로 다시 "Latest"가 됩니다.

## 복구 방법 (`--prerelease` 없이 다른 릴리스가 이미 게시된 경우)

```bash
gh release edit <wpf-dev-pack-tag> --latest
gh release edit <the-other-tag> --prerelease
```

`gh release list`로 `wpf-dev-pack` 행에 `Latest`가 표시되는지
확인하세요.

## `gh` CLI 계정 주의사항

- 이 저장소에는 여러 `gh` 계정이 로그인되어 있을 수 있으며, **활성
  계정**이 항상 저장소 소유자(`christian289`)인 것은 아닙니다. 잘못된
  계정이 활성 상태이면 `gh release create`/`edit`가 애매한 오류(예:
  "workflow scope may be required")로 실패할 수 있습니다.
- 릴리스 작업이 예상치 못하게 실패하면: `gh auth status`로 확인 후
  `gh auth switch --user christian289`로 전환하고 재시도하세요.

## 배경

2026-07-21, `samples/PolyLab3DStudio`를 `polylab3dstudio-v0.1.0`으로
(일반 릴리스로) 배포하면서 `wpf-dev-pack-v1.8.1`이 "Latest" 배지에서
조용히 밀려났습니다. 위 복구 절차로 해결했으며, 재발 방지를 위해 이
규칙을 문서화합니다.
