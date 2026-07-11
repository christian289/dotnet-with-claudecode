namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// The "Windows .NET 3D 도구 지도" screen, transcribed from the
/// ".NET 3D 도구 지도" design screen: a 5-step learning ladder plus an
/// 8-row × 5-tool comparison table. Static content; both CTAs lead to the
/// courses screen where 트랙 B lives.
/// </summary>
public sealed partial class ToolMapViewModel(ShellViewModel shell) : ObservableObject
{
    public ShellViewModel Shell { get; } = shell;

    [RelayCommand]
    private void GoStart() => Shell.GoStart();

    [RelayCommand]
    private void GoCourses() => Shell.GoCourses();

    public IReadOnlyList<ToolMapRow> Rows { get; } =
    [
        new(
            Label: "정체",
            A: "WPF 내장 3D API",
            B: "순정 위의 편의 확장 (렌더링은 여전히 순정)",
            C: "SharpDX(DX11) 커스텀 엔진을 WPF에 삽입",
            D: "저수준 DirectX 바인딩 — 직접 엔진을 짠다",
            E: "오픈소스 3D 게임 엔진 (독립 실행)"),
        new(
            Label: "진입 난이도",
            A: "낮음 — .NET만 알면 시작",
            B: "낮음 — 순정 지식 그대로",
            C: "중간 — 다른 API 표면",
            D: "높음 — 셰이더·파이프라인 직접",
            E: "중간 — 엔진 학습 필요"),
        new(
            Label: "DX 버전",
            A: "내부적으로 D3D9 계열 합성",
            B: "순정과 동일",
            C: "DX11 전용 (SharpDX, 2019 중단)",
            D: "DX11 · DX12 · DXR 모두",
            E: "DX11/12 (엔진이 관리)"),
        new(
            Label: "그림자 / PBR",
            A: "없음 / Phong 계열",
            B: "없음 / Phong 계열",
            C: "있음 / 실제 PBR",
            D: "직접 구현하기 나름 (DXR 가능)",
            E: "있음 / PBR 완비"),
        new(
            Label: "모델 임포터",
            A: "없음 — 직접 파싱",
            B: "OBJ · STL · 3DS (ModelImporter)",
            C: "있음 (Assimp 연동)",
            D: "직접 구현",
            E: "FBX·glTF 등 에디터 지원"),
        new(
            Label: "성능",
            A: "수만 폴리곤에서 한계",
            B: "순정과 동일",
            C: "대량 폴리곤 OK",
            D: "DX12 본연의 성능",
            E: "게임급"),
        new(
            Label: "WPF 삽입 방식",
            A: "순정 (Viewport3D)",
            B: "순정 (HelixViewport3D)",
            C: "엔진 삽입 (전용 Element)",
            D: "D3DImage 브릿지 또는 HwndHost",
            E: "삽입 아님 — 독립 엔진 (범위 밖 참고)"),
        new(
            Label: "언제 넘어가나",
            A: "원리 학습 · 가벼운 3D UI",
            B: "카메라·임포터가 귀찮아질 때",
            C: "그림자·PBR·성능이 필요할 때",
            D: "DXR·메시 셰이더 등 DX11 천장을 넘을 때",
            E: "3D가 앱이 아니라 경험 전체일 때"),
    ];
}
