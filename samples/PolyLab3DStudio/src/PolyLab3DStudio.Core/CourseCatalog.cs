namespace PolyLab3DStudio.Core;

/// <summary>
/// The four learning courses with every lesson, mission, quiz, and reading page,
/// transcribed verbatim from the studio design.
/// </summary>
public static class CourseCatalog
{
    public static IReadOnlyList<Course> All { get; } = Build();

    public static Course? Find(string courseId) => All.FirstOrDefault(c => c.Id == courseId);

    public static Lesson? GetLesson(string courseId, int index)
    {
        Course? course = Find(courseId);
        return course is not null && index >= 0 && index < course.Lessons.Count ? course.Lessons[index] : null;
    }

    public static string LessonKey(string courseId, Lesson lesson) => $"{courseId}.{lesson.Id}";

    private static IReadOnlyList<Course> Build() =>
    [
        new(
            Id: "c1", Num: "01", Title: "3D 공간과 친해지기", English: "MEET THE 3D SPACE",
            Description: "3D 작업의 절반은 \"잘 보는 것\"이에요. 카메라를 자유롭게 움직이고, 첫 오브젝트를 만들어 봅니다.",
            Lessons:
            [
                new(
                    Id: "l1", Kind: LessonKind.Lesson, Title: "카메라 움직이기", English: "MOVING THE CAMERA", Minutes: 5, Seed: "single",
                    Tip: "길을 잃었다면 화면 오른쪽 위 [입체] 버튼을 누르세요. 처음 시점으로 돌아옵니다.",
                    Tasks:
                    [
                        new("뷰포트를 드래그해서 시점을 빙 돌려 보세요", new TaskCheck("action", Action: "orbit")),
                        new("마우스 휠로 확대/축소해 보세요", new TaskCheck("action", Action: "zoom")),
                        new("오른쪽 버튼으로 드래그해서 화면을 옮겨 보세요", new TaskCheck("action", Action: "pan")),
                        new("오른쪽 위 [앞]이나 [위] 버튼으로 시점을 바꿔 보세요", new TaskCheck("action", Action: "preset")),
                    ]),
                new(
                    Id: "l2", Kind: LessonKind.Lesson, Title: "만들고, 고르고, 지우기", English: "CREATE · SELECT · DELETE", Minutes: 6, Seed: "empty",
                    Tip: "빈 곳을 클릭하면 선택이 풀려요. 왼쪽 [장면] 목록에서도 선택할 수 있어요.",
                    Tasks:
                    [
                        new("위 [만들기]에서 큐브를 추가해 보세요", new TaskCheck("add", Shape: ShapeKind.Cube)),
                        new("이번엔 구(Sphere)를 하나 추가해 보세요", new TaskCheck("add", Shape: ShapeKind.Sphere)),
                        new("오브젝트를 클릭해서 선택해 보세요", new TaskCheck("select")),
                        new("선택한 오브젝트를 삭제해 보세요 (Delete 키)", new TaskCheck("delete")),
                    ]),
                new(
                    Id: "q", Kind: LessonKind.Quiz, Title: "개념 체크: 3D 공간", English: "QUIZ", Minutes: 3,
                    Questions:
                    [
                        new("3D 공간에서 위·아래(높이)를 나타내는 축은 무엇일까요?", ["X축", "Y축", "Z축"], 1,
                            "폴리랩에서는 초록색 Y축이 위쪽 방향이에요. X는 좌우, Z는 앞뒤를 나타냅니다."),
                        new("마우스 휠을 굴리면 어떤 일이 일어나나요?", ["시점이 회전한다", "화면이 확대/축소된다", "오브젝트가 복제된다"], 1,
                            "휠은 카메라를 가까이/멀리 옮기는 확대·축소(Zoom)예요."),
                        new("오브젝트를 선택하는 가장 기본적인 방법은?", ["빈 공간을 드래그한다", "오브젝트를 클릭한다", "휠을 꾹 누른다"], 1,
                            "클릭 한 번이면 선택! 빈 곳을 클릭하면 선택이 풀립니다."),
                    ]),
            ]),
        new(
            Id: "c2", Num: "02", Title: "변형 3총사: 이동·회전·크기", English: "MOVE · ROTATE · SCALE",
            Description: "모든 3D 도구의 심장인 세 가지 변형 도구를 익히고, 첫 작품 눈사람을 만들어 봅니다.",
            Lessons:
            [
                new(
                    Id: "l1", Kind: LessonKind.Lesson, Title: "이동 도구 (W)", English: "MOVE TOOL", Minutes: 5, Seed: "single",
                    Tip: "위로 올릴 때는 Shift를 누른 채 드래그! 대부분의 3D 도구에 비슷한 개념이 있어요.",
                    Tasks:
                    [
                        new("이동 도구를 켜 보세요 (W 키 또는 툴바)", new TaskCheck("tool", Tool: ToolCatalog.Move)),
                        new("큐브를 드래그해서 바닥 위에서 옮겨 보세요", new TaskCheck("transform", Mode: "move")),
                        new("Shift를 누른 채 드래그해서 공중으로 띄워 보세요", new TaskCheck("transform", Mode: "move", Shift: true)),
                    ]),
                new(
                    Id: "l2", Kind: LessonKind.Lesson, Title: "회전과 크기 (E · R)", English: "ROTATE & SCALE", Minutes: 5, Seed: "single",
                    Tip: "오른쪽 속성 패널에서 숫자로 정확하게 조절할 수도 있어요.",
                    Tasks:
                    [
                        new("회전 도구를 켜 보세요 (E)", new TaskCheck("tool", Tool: ToolCatalog.Rotate)),
                        new("오브젝트를 좌우로 드래그해서 돌려 보세요", new TaskCheck("transform", Mode: "rotate")),
                        new("크기 도구를 켜 보세요 (R)", new TaskCheck("tool", Tool: ToolCatalog.Scale)),
                        new("오브젝트를 위아래로 드래그해서 크기를 바꿔 보세요", new TaskCheck("transform", Mode: "scale")),
                    ]),
                new(
                    Id: "l3", Kind: LessonKind.Mission, Title: "미션: 눈사람 만들기", English: "MISSION: SNOWMAN", Minutes: 8, Seed: "empty",
                    Tip: "몸통 → 머리 → 코 순서가 편해요. 흰색은 재질 색에서 고를 수 있어요.",
                    Tasks:
                    [
                        new("구를 3개 만들어 보세요", new TaskCheck("count", Shape: ShapeKind.Sphere, Count: 3)),
                        new("크기 도구(R)로 구들의 크기를 서로 다르게 해 보세요", new TaskCheck("transform", Mode: "scale")),
                        new("이동 도구(W) + Shift 드래그로 위로 쌓아 보세요", new TaskCheck("transform", Mode: "move", Shift: true)),
                        new("원뿔을 추가해서 코를 달아 주세요", new TaskCheck("add", Shape: ShapeKind.Cone)),
                        new("속성 패널에서 마음에 드는 색을 입혀 보세요", new TaskCheck("color")),
                    ]),
                new(
                    Id: "q", Kind: LessonKind.Quiz, Title: "개념 체크: 변형", English: "QUIZ", Minutes: 3,
                    Questions:
                    [
                        new("단축키 W는 어떤 도구인가요?", ["선택", "이동", "회전"], 1,
                            "Q 선택, W 이동, E 회전, R 크기 — 많은 3D 도구가 비슷한 배치를 써요."),
                        new("오브젝트를 공중으로 띄우려면 이동 중에 무엇을 누르나요?", ["Shift", "Ctrl", "Tab"], 0,
                            "Shift+드래그가 위아래 이동이에요."),
                        new("크기 도구의 단축키는?", ["E", "R", "S"], 1,
                            "R(크기)! E는 회전이었죠."),
                    ]),
            ]),
        new(
            Id: "c3", Num: "03", Title: "재질과 빛", English: "MATERIALS & LIGHT",
            Description: "같은 모양도 재질과 빛에 따라 완전히 달라 보여요. 색·거칠기·금속성, 그리고 조명을 다뤄 봅니다.",
            Lessons:
            [
                new(
                    Id: "l1", Kind: LessonKind.Lesson, Title: "색과 재질", English: "COLOR & MATERIAL", Minutes: 5, Seed: "two",
                    Tip: "거칠기(Roughness)가 낮을수록 매끈하고 반짝여요. 금속성(Metalness)을 올리면 금속처럼 비칩니다.",
                    Tasks:
                    [
                        new("오브젝트 하나를 선택해 보세요", new TaskCheck("select")),
                        new("속성 패널에서 색을 바꿔 보세요", new TaskCheck("color")),
                        new("거칠기 슬라이더를 움직여 보세요", new TaskCheck("slider", Key: "roughness")),
                        new("금속성 슬라이더도 움직여 보세요", new TaskCheck("slider", Key: "metalness")),
                    ]),
                new(
                    Id: "l2", Kind: LessonKind.Lesson, Title: "빛 다루기", English: "WORKING WITH LIGHT", Minutes: 4, Seed: "lit",
                    Tip: "그림자가 어느 쪽으로 지는지 살펴보세요. 빛의 방향이 장면의 분위기를 만듭니다.",
                    Tasks:
                    [
                        new("조명 [밝기] 슬라이더를 움직여 보세요", new TaskCheck("light", Key: "intensity")),
                        new("조명 [방향] 슬라이더로 그림자를 옮겨 보세요", new TaskCheck("light", Key: "angle")),
                        new("시점을 돌려서 여러 각도에서 살펴 보세요", new TaskCheck("action", Action: "orbit")),
                    ]),
                new(
                    Id: "q", Kind: LessonKind.Quiz, Title: "개념 체크: 재질과 빛", English: "QUIZ", Minutes: 3,
                    Questions:
                    [
                        new("거칠기(Roughness)를 낮추면 표면은 어떻게 보일까요?", ["더 반짝인다", "더 어두워진다", "더 커진다"], 0,
                            "거칠기가 낮다 = 표면이 매끈하다 = 빛을 잘 반사해서 반짝여요."),
                        new("금속성(Metalness)을 최대로 올리면?", ["투명해진다", "금속처럼 보인다", "그림자가 사라진다"], 1,
                            "금속성은 표면이 금속처럼 빛을 반사하는 정도예요."),
                        new("빛의 방향을 바꾸면 함께 움직이는 것은?", ["그림자", "오브젝트", "카메라"], 0,
                            "빛의 반대편으로 그림자가 드리워져요."),
                    ]),
            ]),
        new(
            Id: "c4", Num: "04", Title: "WPF로 3D 그리기", English: "WPF 3D FOR .NET DEVELOPERS",
            Description: "폴리랩에서 배운 개념이 .NET/WPF 코드로 어떻게 옮겨지는지 배워요. Viewport3D부터 Transform3DGroup까지, 그리고 내 장면을 직접 코드로 내보내 봅니다.",
            Lessons:
            [
                new(
                    Id: "r1", Kind: LessonKind.Reading, Title: "WPF 3D의 뼈대", English: "VIEWPORT3D & SCENE TREE", Minutes: 5,
                    Pages:
                    [
                        new(
                            "Viewport3D — 3D가 그려지는 창",
                            [
                                "WPF에서 3D는 Viewport3D라는 컨트롤 안에 그려져요. 폴리랩 가운데의 뷰포트가 바로 이 역할이에요.",
                                "Viewport3D는 평범한 XAML 컨트롤이라 Grid 안에 넣고, 버튼·패널과 함께 배치할 수 있어요. 별도 라이브러리 없이 .NET에 기본 포함되어 있습니다.",
                            ],
                            """
                            <Grid>
                                <Viewport3D>
                                    <Viewport3D.Camera>
                                        <PerspectiveCamera x:Name="Camera" FieldOfView="50"/>
                                    </Viewport3D.Camera>
                                    <ModelVisual3D x:Name="SceneRoot"/>
                                </Viewport3D>
                            </Grid>
                            """),
                        new(
                            "장면은 트리 — ModelVisual3D",
                            [
                                "폴리랩 왼쪽의 [장면] 목록처럼, WPF 3D도 ModelVisual3D 노드를 트리로 쌓아서 장면을 만들어요.",
                                "SceneRoot.Children.Add(...)를 호출하는 것이 폴리랩의 [만들기] 버튼과 정확히 같은 일이에요. 오브젝트도, 조명도 모두 이 트리의 노드입니다.",
                            ],
                            """
                            // 오브젝트 추가 = 트리에 노드 추가
                            SceneRoot.Children.Add(new ModelVisual3D { Content = model });

                            // 조명도 같은 방식!
                            SceneRoot.Children.Add(new ModelVisual3D {
                                Content = new AmbientLight(Color.FromRgb(72, 78, 86))
                            });
                            """),
                        new(
                            "카메라 — PerspectiveCamera",
                            [
                                "Position(카메라 위치), LookDirection(바라보는 방향), FieldOfView(화각) 세 가지만 알면 돼요.",
                                "폴리랩에서 드래그로 시점을 돌리는 것은, 사실 이 값들을 매 프레임 다시 계산해 넣는 것이에요. [XAML 내보내기]의 UpdateCamera()에서 실제 계산식을 볼 수 있어요.",
                            ],
                            """
                            Camera.Position = new Point3D(6, 5, 8);
                            Camera.LookDirection = new Vector3D(-6, -4.3, -8); // 원점을 향해
                            Camera.UpDirection = new Vector3D(0, 1, 0);        // Y가 위
                            """),
                    ]),
                new(
                    Id: "r2", Kind: LessonKind.Reading, Title: "메시와 재질", English: "MESHGEOMETRY3D & MATERIALS", Minutes: 5,
                    Pages:
                    [
                        new(
                            "MeshGeometry3D — 점과 삼각형",
                            [
                                "모든 입체는 정점 목록(Positions)과 삼각형 목록(TriangleIndices)으로 정의돼요. 폴리랩의 큐브도 삼각형 12개로 이루어져 있어요.",
                                "TriangleIndices는 3개씩 끊어 읽어요 — \"0, 1, 2\"는 0·1·2번 정점으로 삼각형 하나를 만들라는 뜻이에요.",
                            ],
                            """
                            var m = new MeshGeometry3D();
                            m.Positions.Add(new Point3D(0, 0, 0)); // 0번 정점
                            m.Positions.Add(new Point3D(1, 0, 0)); // 1번 정점
                            m.Positions.Add(new Point3D(0, 1, 0)); // 2번 정점
                            m.TriangleIndices.Add(0);
                            m.TriangleIndices.Add(1);
                            m.TriangleIndices.Add(2); // → 삼각형 1개 완성
                            """),
                        new(
                            "GeometryModel3D — 모양 + 재질",
                            [
                                "메시(모양)에 Material(재질)을 입힌 것이 GeometryModel3D예요. 폴리랩의 재질 패널이 여기에 대응돼요.",
                                "DiffuseMaterial은 기본 색, SpecularMaterial은 반짝임(폴리랩의 거칠기를 낮춘 효과), EmissiveMaterial은 스스로 빛나는 효과예요. MaterialGroup으로 겹쳐 쓸 수 있어요.",
                            ],
                            """
                            var mat = new MaterialGroup();
                            mat.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
                            mat.Children.Add(new SpecularMaterial(Brushes.White, 60)); // 반짝!

                            var model = new GeometryModel3D(mesh, mat);
                            """),
                    ]),
                new(
                    Id: "r3", Kind: LessonKind.Reading, Title: "변형과 조명", English: "TRANSFORMS & LIGHTS", Minutes: 5,
                    Pages:
                    [
                        new(
                            "Transform3DGroup — W·E·R의 정체",
                            [
                                "폴리랩의 이동(W)·회전(E)·크기(R)는 WPF에서 각각 TranslateTransform3D, RotateTransform3D, ScaleTransform3D예요.",
                                "Transform3DGroup으로 묶으면 순서대로 적용돼요. 보통 크기 → 회전 → 이동 순서를 써요. 순서를 바꾸면 결과가 달라지니 주의!",
                            ],
                            """
                            var tg = new Transform3DGroup();
                            tg.Children.Add(new ScaleTransform3D(s, s, s));
                            tg.Children.Add(new RotateTransform3D(
                                new AxisAngleRotation3D(new Vector3D(0, 1, 0), 45))); // Y축 45도
                            tg.Children.Add(new TranslateTransform3D(x, y, z));
                            model.Transform = tg;
                            """),
                        new(
                            "조명 — 빛이 없으면 검은 화면",
                            [
                                "WPF 3D는 빛이 없으면 아무것도 안 보여요. DirectionalLight(한 방향에서 오는 태양빛)와 AmbientLight(전체를 은은하게 밝히는 빛)를 함께 쓰는 것이 기본이에요.",
                                "폴리랩 [조명] 패널의 밝기·방향 슬라이더가 곧 DirectionalLight의 Color와 Direction 값이에요.",
                            ],
                            """
                            // 태양빛: 방향 벡터가 "빛이 나아가는 방향"
                            SceneRoot.Children.Add(new ModelVisual3D {
                                Content = new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1))
                            });
                            // 은은한 바탕 빛
                            SceneRoot.Children.Add(new ModelVisual3D {
                                Content = new AmbientLight(Color.FromRgb(72, 78, 86))
                            });
                            """),
                    ]),
                new(
                    Id: "m4", Kind: LessonKind.Mission, Title: "미션: 내 장면을 WPF 코드로", English: "MISSION: EXPORT TO WPF", Minutes: 6, Seed: "single",
                    Tip: "내보낸 코드의 BuildScene()을 보면, 방금 만든 도형들이 AddObject(...) 한 줄씩으로 나타나 있어요.",
                    Tasks:
                    [
                        new("도형을 하나 추가해 보세요 (포인트 클라우드도 좋아요)", new TaskCheck("add")),
                        new("이동·회전·크기로 마음대로 배치해 보세요", new TaskCheck("transform")),
                        new("상단의 [XAML] 버튼으로 내보내기를 열어 보세요", new TaskCheck("action", Action: "xaml")),
                        new("[MainWindow.xaml.cs] 탭에서 내 도형 코드를 확인해 보세요", new TaskCheck("action", Action: "cstab")),
                        new("[코드 복사]를 눌러 Visual Studio로 가져갈 준비!", new TaskCheck("action", Action: "copy")),
                    ]),
                new(
                    Id: "q", Kind: LessonKind.Quiz, Title: "개념 체크: WPF 3D", English: "QUIZ", Minutes: 3,
                    Questions:
                    [
                        new("3D 장면이 그려지는 WPF 컨트롤은 무엇일까요?", ["Viewport3D", "Canvas", "RenderPanel"], 0,
                            "Canvas는 2D 전용이에요. 3D는 Viewport3D 안에서만 그려집니다."),
                        new("입체의 모양(정점·삼각형)을 담는 타입은?", ["GeometryModel3D", "MeshGeometry3D", "ModelVisual3D"], 1,
                            "Mesh가 모양, GeometryModel3D는 모양+재질, ModelVisual3D는 장면 트리의 노드예요."),
                        new("이동·회전·크기를 한 오브젝트에 함께 적용하려면?", ["Transform3DGroup", "Storyboard", "MatrixStack"], 0,
                            "세 변형을 Transform3DGroup에 순서대로 담아 model.Transform에 넣어요."),
                        new("WPF 3D 장면에서 빛을 하나도 넣지 않으면?", ["기본 조명이 자동 적용된다", "화면에 아무것도 안 보인다", "컴파일 에러가 난다"], 1,
                            "빛도 장면 트리의 노드예요. DirectionalLight + AmbientLight 조합이 기본이에요."),
                    ]),
            ]),
    ];
}
