# Advanced FlowDocument Patterns

## Creating Documents in Code

### Building FlowDocument Programmatically

```csharp
namespace MyApp.Documents;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

public static class DocumentBuilder
{
    public static FlowDocument CreateReport(string title, IEnumerable<ReportItem> items)
    {
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            PagePadding = new Thickness(50)
        };

        // Title
        // 제목
        document.Blocks.Add(new Paragraph(new Run(title))
        {
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        // Table
        // 테이블
        var table = CreateTable(items);
        document.Blocks.Add(table);

        return document;
    }

    private static Table CreateTable(IEnumerable<ReportItem> items)
    {
        var table = new Table { CellSpacing = 0 };

        // Columns
        // 열
        table.Columns.Add(new TableColumn { Width = new GridLength(200) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });

        // Header
        // 헤더
        var headerGroup = new TableRowGroup();
        var headerRow = new TableRow { Background = Brushes.LightGray };
        headerRow.Cells.Add(CreateCell("Item", FontWeights.Bold));
        headerRow.Cells.Add(CreateCell("Quantity", FontWeights.Bold));
        headerRow.Cells.Add(CreateCell("Price", FontWeights.Bold));
        headerGroup.Rows.Add(headerRow);
        table.RowGroups.Add(headerGroup);

        // Data rows
        // 데이터 행
        var dataGroup = new TableRowGroup();
        var alternate = false;

        foreach (var item in items)
        {
            var row = new TableRow();

            if (alternate)
            {
                row.Background = new SolidColorBrush(Color.FromRgb(248, 248, 248));
            }

            row.Cells.Add(CreateCell(item.Name));
            row.Cells.Add(CreateCell(item.Quantity.ToString()));
            row.Cells.Add(CreateCell($"${item.Price:F2}"));

            dataGroup.Rows.Add(row);
            alternate = !alternate;
        }

        table.RowGroups.Add(dataGroup);
        return table;
    }

    private static TableCell CreateCell(string text, FontWeight? fontWeight = null)
    {
        var paragraph = new Paragraph(new Run(text))
        {
            Margin = new Thickness(5)
        };

        if (fontWeight.HasValue)
        {
            paragraph.FontWeight = fontWeight.Value;
        }

        return new TableCell(paragraph);
    }
}

public record ReportItem(string Name, int Quantity, decimal Price);
```

---

## Printing FlowDocument

```csharp
namespace MyApp.Services;

using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

public static class DocumentPrinter
{
    public static void Print(FlowDocument document, string description = "Document")
    {
        var printDialog = new PrintDialog();

        if (printDialog.ShowDialog() != true)
            return;

        // Clone document for printing (FlowDocument can only have one parent)
        // 인쇄용 문서 복제 (FlowDocument는 부모가 하나만 가능)
        var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;

        // Set page size from printer
        // 프린터에서 페이지 크기 설정
        paginator.PageSize = new Size(
            printDialog.PrintableAreaWidth,
            printDialog.PrintableAreaHeight);

        printDialog.PrintDocument(paginator, description);
    }

    public static void PrintPreview(FlowDocument document)
    {
        var window = new Window
        {
            Title = "Print Preview",
            Width = 800,
            Height = 600,
            Content = new FlowDocumentReader
            {
                Document = document,
                ViewingMode = FlowDocumentReaderViewingMode.Page
            }
        };

        window.ShowDialog();
    }
}
```

---

## Loading and Saving

### Save to XAML

```csharp
using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;

public static void SaveToXaml(FlowDocument document, string filePath)
{
    using var stream = new FileStream(filePath, FileMode.Create);
    XamlWriter.Save(document, stream);
}

public static FlowDocument LoadFromXaml(string filePath)
{
    using var stream = new FileStream(filePath, FileMode.Open);
    return (FlowDocument)XamlReader.Load(stream);
}
```

### Save to XPS

```csharp
using System.IO;
using System.IO.Packaging;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

public static void SaveToXps(FlowDocument document, string filePath)
{
    using var package = Package.Open(filePath, FileMode.Create);
    using var xpsDocument = new XpsDocument(package, CompressionOption.Maximum);

    var writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
    writer.Write(((IDocumentPaginatorSource)document).DocumentPaginator);
}
```
