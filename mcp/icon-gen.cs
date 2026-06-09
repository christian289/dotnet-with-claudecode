#:package System.Drawing.Common@9.0.0
#:property TargetFramework=net10.0-windows
// Icon generator for WpfDevPackMcp (Windows / GDI+ for real font rendering).
// Standalone file-based app — NOT part of the WpfDevPackMcp build (excluded via
// <Compile Remove> in the .csproj). Regenerate the icon with:
//   dotnet mcp/icon-gen.cs mcp/icon.png
// Design: .NET-purple rounded tile + WPF app window (title bar dots) + XAML </>
// markup + a "wpf-dev-pack" wordmark.
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

string outPath = args.Length > 0 ? args[0] : "icon.png";
const int S = 256;

using var bmp = new Bitmap(S, S, PixelFormat.Format32bppArgb);
using var g = Graphics.FromImage(bmp);
g.SmoothingMode = SmoothingMode.AntiAlias;
g.PixelOffsetMode = PixelOffsetMode.HighQuality;
g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
g.Clear(Color.Transparent);

static GraphicsPath RoundRect(float x, float y, float w, float h, float r)
{
    var p = new GraphicsPath();
    float d = 2 * r;
    p.AddArc(x, y, d, d, 180, 90);
    p.AddArc(x + w - d, y, d, d, 270, 90);
    p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
    p.AddArc(x, y + h - d, d, d, 90, 90);
    p.CloseFigure();
    return p;
}

// rounded .NET-purple tile with a diagonal gradient
using (var tile = RoundRect(0, 0, S, S, 56))
using (var br = new LinearGradientBrush(new Point(0, 0), new Point(S, S),
        Color.FromArgb(67, 36, 168), Color.FromArgb(130, 94, 255)))
{
    g.FillPath(br, tile);
}

// soft drop shadow + white app window (raised toward the top)
using (var shadow = RoundRect(53, 56, 150, 96, 16))
using (var sb = new SolidBrush(Color.FromArgb(60, 18, 8, 55)))
{
    g.FillPath(sb, shadow);
}
var winPath = RoundRect(53, 50, 150, 96, 16);
using (var wb = new SolidBrush(Color.White))
{
    g.FillPath(wb, winPath);
}

// title bar (clip to the window so corners stay rounded) + traffic-light dots
var savedClip = g.Clip;
g.SetClip(winPath);
using (var tbBrush = new SolidBrush(Color.FromArgb(231, 224, 250)))
{
    g.FillRectangle(tbBrush, 53, 50, 150, 26);
}
g.Clip = savedClip;
void Dot(float cx, float cy, Color c)
{
    using var b = new SolidBrush(c);
    g.FillEllipse(b, cx - 4.5f, cy - 4.5f, 9, 9);
}
Dot(72, 63, Color.FromArgb(34, 211, 238));
Dot(88, 63, Color.FromArgb(139, 92, 246));
Dot(104, 63, Color.FromArgb(244, 114, 182));

// XAML </> markup in the window body
using (var purple = new Pen(Color.FromArgb(81, 43, 212), 9f)
{ StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
using (var cyan = new Pen(Color.FromArgb(34, 211, 238), 9f)
{ StartCap = LineCap.Round, EndCap = LineCap.Round })
{
    g.DrawLines(purple, [new PointF(98, 86), new PointF(80, 108), new PointF(98, 130)]);   // <
    g.DrawLines(purple, [new PointF(158, 86), new PointF(176, 108), new PointF(158, 130)]); // >
    g.DrawLine(cyan, 118, 132, 138, 84);                                                    // /
}

// "wpf-dev-pack" wordmark beneath the window (auto-sized to a target width)
const string text = "wpf-dev-pack";
const float targetWidth = 206f;
using (var family = new FontFamily("Segoe UI"))
{
    using var probe = new Font(family, 40f, FontStyle.Bold, GraphicsUnit.Pixel);
    SizeF m = g.MeasureString(text, probe);
    float fontSize = 40f * (targetWidth / m.Width);
    using var font = new Font(family, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
    using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    using var tb = new SolidBrush(Color.White);
    g.DrawString(text, font, tb, new RectangleF(0, 176, S, 56), fmt);
}

winPath.Dispose();
bmp.Save(outPath, ImageFormat.Png);
Console.WriteLine($"wrote {outPath} {S}x{S}");
