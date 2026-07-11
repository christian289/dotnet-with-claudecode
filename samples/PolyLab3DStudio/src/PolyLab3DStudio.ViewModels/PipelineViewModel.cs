namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// The "WPF 코어: 화면에 그려지기까지" screen, transcribed from the
/// "WPF 렌더링 파이프라인" design screen: six selectable stages across the
/// UI-thread and render-thread lanes plus a detail panel for the selection.
/// </summary>
public sealed partial class PipelineViewModel : ObservableObject
{
    public PipelineViewModel(ShellViewModel shell)
    {
        Shell = shell;

        PipelineStageViewModel[] stages =
        [
            new(
                num: "1", name: "코드 (XAML · C#)", sub: "여러분이 작성하는 것",
                icon: "M8 6l-5 6 5 6M16 6l5 6-5 6M13 4l-2 16",
                isUiLane: true, isLastInLane: false,
                paras:
                [
                    "XAML 파서와 C# 코드가 객체를 만들어요. Button도, Viewport3D도, MeshGeometry3D도 이 단계에서는 그냥 .NET 객체입니다.",
                    "이 단계의 비용은 \"객체 생성\"이에요. 거대한 메시를 매 프레임 새로 만들면 여기서부터 느려집니다 — 폴리랩이 오브젝트를 재사용하는 이유예요.",
                ],
                select: Select),
            new(
                num: "2", name: "논리 트리", sub: "Logical Tree — 구조와 상속",
                icon: "M12 3v5M12 8L5 13M12 8l7 5M12 3a2 2 0 100 .01M5 13a2 2 0 100 4 2 2 0 000-4zM19 13a2 2 0 100 4 2 2 0 000-4zM12 13a2 2 0 100 4 2 2 0 000-4z",
                isUiLane: true, isLastInLane: false,
                paras:
                [
                    "Window → Grid → Viewport3D처럼 요소의 소속 관계를 담는 트리예요. DataContext 상속, 리소스 탐색, 이벤트 라우팅이 이 트리를 따라 움직여요.",
                    "아직 \"어떻게 보이는가\"의 정보는 없어요. 그건 다음 단계의 일입니다.",
                ],
                select: Select),
            new(
                num: "3", name: "비주얼 트리", sub: "Visual Tree — 그려질 모든 것",
                icon: "M3 8l9-5 9 5-9 5-9-5zM3 12l9 5 9-5M3 16l9 5 9-5",
                isUiLane: true, isLastInLane: true,
                paras:
                [
                    "템플릿이 펼쳐진, 실제로 그려질 Visual들의 트리예요. Button 하나가 Border·ContentPresenter·TextBlock으로 확장돼요. Viewport3D도 이 트리의 Visual 하나입니다.",
                    "측정(Measure)·배치(Arrange)·히트 테스트가 이 트리에서 일어나요. VisualTreeHelper.HitTest로 3D 피킹을 하는 것도 이 단계의 API예요.",
                ],
                select: Select),
            new(
                num: "4", name: "milcore 합성 트리", sub: "Composition Tree — 복제본",
                icon: "M4 4h9v9H4zM11 11h9v9h-9z",
                isUiLane: false, isLastInLane: false,
                paras:
                [
                    "스레드 경계(DUCE 채널)를 건너, 비주얼 트리의 렌더링 명세가 네이티브 합성 엔진 milcore의 트리로 복제돼요. 여기서부터는 관리 코드가 아니라 우리가 만질 수 없는 영역입니다.",
                    "UI 스레드가 바쁜 동안에도 렌더 스레드는 이 복제본으로 애니메이션을 계속 돌릴 수 있어요. Freeze()한 리소스는 \"다시는 안 바뀐다\"는 약속이라, 채널 동기화 비용이 사라져 성능이 좋아져요.",
                ],
                select: Select),
            new(
                num: "5", name: "DirectX", sub: "래스터화 — 삼각형 → 픽셀",
                icon: "M7 7h10v10H7zM7 3v4M12 3v4M17 3v4M7 17v4M12 17v4M17 17v4M3 7h4M3 12h4M3 17h4M17 7h4M17 12h4M17 17h4",
                isUiLane: false, isLastInLane: false,
                paras:
                [
                    "milcore가 합성 트리를 DirectX 명령으로 바꿔 GPU에 보내요. 2D 도형도, Viewport3D의 삼각형도 결국 여기서 래스터화됩니다.",
                    "조명이 너무 많거나 GPU가 감당 못 하면 이 단계가 소프트웨어 렌더링으로 후퇴해요 — 트랙 A-3에서 본 \"빛도 공짜가 아니다\"의 현장이에요.",
                ],
                select: Select),
            new(
                num: "6", name: "화면", sub: "Present — 모니터로",
                icon: "M3 4h18v12H3zM9 20h6M12 16v4",
                isUiLane: false, isLastInLane: true,
                paras:
                [
                    "완성된 프레임이 화면에 프레젠트돼요. WPF는 보통 모니터 주사율에 맞춰 이 사이클을 반복합니다.",
                    "외부 DX12 콘텐츠가 이 화면에 들어오려면 D3DImage 브릿지(WPF와 합성) 또는 HwndHost(직접 프레젠트) 중 하나를 골라야 해요 — 아래 카드 참고!",
                ],
                select: Select),
        ];

        UiStages = [.. stages[..3]];
        RenderStages = [.. stages[3..]];
        _selected = stages[0];
        _selected.IsSelected = true;
    }

    public ShellViewModel Shell { get; }

    public IReadOnlyList<PipelineStageViewModel> UiStages { get; }

    public IReadOnlyList<PipelineStageViewModel> RenderStages { get; }

    [ObservableProperty] private PipelineStageViewModel _selected;

    private void Select(PipelineStageViewModel stage)
    {
        if (stage == Selected)
        {
            return;
        }

        Selected.IsSelected = false;
        stage.IsSelected = true;
        Selected = stage;
    }

    [RelayCommand]
    private void GoStart() => Shell.GoStart();

    [RelayCommand]
    private void GoCourses() => Shell.GoCourses();
}
