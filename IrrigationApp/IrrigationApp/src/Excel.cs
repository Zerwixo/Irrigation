using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CsvHelper;

namespace IrrigationApp;

public static class Excel
{
    public static async Task ExportAsync(TopLevel owner, IEnumerable<Data> data)
    {
        if (owner.StorageProvider is null)
        {
            return;
        }

        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Enregistrer l'export",
            SuggestedFileName = $"export-{System.DateTime.Now:yyyy-MM-dd}",
            DefaultExtension = "csv",
            FileTypeChoices =
            [
                new FilePickerFileType("CSV")
                {
                    Patterns = ["*.csv"]
                }
            ]
        });

        if (file is null)
        {
            return;
        }

        await SaveAsync(file, data);
    }

    public static async Task<bool> SaveAsync(IStorageFile file, IEnumerable<Data> data)
    {
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);

        csv.WriteRecords(data);
        await writer.FlushAsync();
        return true;
    }
}
