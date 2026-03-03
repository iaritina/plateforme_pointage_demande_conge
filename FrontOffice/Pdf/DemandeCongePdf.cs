using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.models;

public class DemandeCongePdf : IDocument
{
    private readonly IList<DemandeConge> _items;

    public DemandeCongePdf(IList<DemandeConge> items) => _items = items;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken4));

            page.Header().Element(Header);

            page.Content().PaddingTop(12).Element(Content);

            page.Footer()
                .AlignRight()
                .DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Darken1))
                .Text(t =>
                {
                    t.Span("Généré le ");
                    t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
        });
    }

    void Header(IContainer container)
    {
        container
            .PaddingBottom(10)
            .Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item()
                        .Text("Mes demandes de congé")
                        .FontSize(18)
                        .SemiBold()
                        .FontColor(Colors.Black);

                    col.Item()
                        .PaddingTop(3)
                        .Text("Historique de vos absences")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(120)
                    .AlignRight()
                    .Text($"Total : {_items.Count}")
                    .FontSize(10)
                    .SemiBold();
            });
    }

    void Content(IContainer container)
    {
        container
            .Background(Colors.White)
            .Border(1).BorderColor(Colors.Grey.Lighten2)
            .CornerRadius(8)
            .Padding(12)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Période
                    columns.RelativeColumn(3); // Motif
                    columns.RelativeColumn(1); // Durée
                    columns.RelativeColumn(1); // Statut
                    columns.RelativeColumn(1); // Année
                });

                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Période");
                    HeaderCell(h.Cell(), "Motif");
                    HeaderCell(h.Cell().AlignCenter(), "Durée");
                    HeaderCell(h.Cell(), "Statut");
                    HeaderCell(h.Cell().AlignCenter(), "Année");
                });

                if (_items.Count == 0)
                {
                    table.Cell().ColumnSpan(5)
                        .PaddingVertical(20)
                        .AlignCenter()
                        .Text("Aucune demande de congé.")
                        .FontColor(Colors.Grey.Darken1);
                    return;
                }

                for (int i = 0; i < _items.Count; i++)
                {
                    var d = _items[i];
                    var zebra = i % 2 == 0;

                    RowCell(table.Cell(), zebra, $"{d.DateDebut:dd MMM yyyy} → {d.DateFin:dd MMM yyyy}");
                    RowCell(table.Cell(), zebra, Truncate(d.Motif ?? "", 140));
                    RowCell(table.Cell().AlignCenter(), zebra, $"{d.NombreJour:0.##} j");
                    StatusCell(table.Cell(), zebra, d.Status);
                    RowCell(table.Cell().AlignCenter(), zebra, d.decisionYear.ToString());
                }
            });
    }

    static void HeaderCell(IContainer container, string text)
    {
        container
            .Background(Colors.Grey.Lighten4)
            .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(8).PaddingHorizontal(8)
            .Text(text)
            .SemiBold()
            .FontSize(9)
            .FontColor(Colors.Grey.Darken2);
    }

    static void RowCell(IContainer container, bool zebra, string text)
    {
        container
            .Background(zebra ? Colors.Grey.Lighten5 : Colors.White)
            .BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(8).PaddingHorizontal(8)
            .Text(text);
    }

    // ✅ Statut en texte (stable, pas de conflit de largeur)
    static void StatusCell(IContainer container, bool zebra, StatusEnum status)
    {
        var (fg, label) = status switch
        {
            StatusEnum.pending => (Colors.Brown.Darken2, "En attente"),
            StatusEnum.ok      => (Colors.Green.Darken3, "Acceptée"),
            StatusEnum.ko      => (Colors.Red.Darken3, "Refusée"),
            _                  => (Colors.Grey.Darken3, status.ToString())
        };

        container
            .Background(zebra ? Colors.Grey.Lighten5 : Colors.White)
            .BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(8).PaddingHorizontal(8)
            .Text(label)
            .FontSize(9)
            .SemiBold()
            .FontColor(fg);
    }

    static string Truncate(string input, int max)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        input = input.Trim();
        return input.Length <= max ? input : input.Substring(0, max) + "…";
    }
}