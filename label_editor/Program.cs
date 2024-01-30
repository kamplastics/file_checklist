using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

class Program
{
    static void Main(string[] args)
    {
        string csvFilePath = "ff.csv";
        List<LabelRecord> records = ReadCsvFile(csvFilePath);
        int selectedIndex = 0;

        while (true)
        {
            DisplayRecords(records, selectedIndex);
            Console.WriteLine("Type 'j' or 'k' to navigate, 'g' to jump to a row, 'e' to edit, or 'q' to quit:");

            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // 'true' to not display the pressed key

            switch (keyInfo.Key)
            {
                case ConsoleKey.Q:
                    return; // Exit the loop

                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                    selectedIndex = Math.Min(records.Count - 1, selectedIndex + 1);
                    break;

                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;

                case ConsoleKey.Home:
                    selectedIndex = 0; // Jump to the first record
                    break;

                case ConsoleKey.End:
                    selectedIndex = records.Count - 1; // Jump to the last record
                    break;

                case ConsoleKey.PageUp:
                    selectedIndex = Math.Max(0, selectedIndex - 20); // Move up by 20 records
                    break;

                case ConsoleKey.PageDown:
                    selectedIndex = Math.Min(records.Count - 1, selectedIndex + 20); // Move down by 20 records
                    break;

                case ConsoleKey.G:
                    Console.WriteLine("\nEnter the row number you wish to jump to (or 'c' to cancel):");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out int rowNumber) && rowNumber >= 1 && rowNumber <= records.Count)
                    {
                        selectedIndex = rowNumber - 1; // Adjust for zero-based index
                    }
                    else if (input.ToLower() != "c")
                    {
                        Console.WriteLine("Invalid row number. Press any key to continue...");
                        Console.ReadKey(intercept: true);
                    }
                    break; 

                case ConsoleKey.E:
                    // 'e' to edit the current record
                    EditRecord(records, selectedIndex, csvFilePath);
                    break;
                case ConsoleKey.H:
                    DisplayHelp();
                    break;

                case ConsoleKey.Spacebar:
                    // Toggle the 'checked' status of the current record
                    records[selectedIndex].Checked = !records[selectedIndex].Checked;
                    // Update the CSV file
                    UpdateCsvFile(csvFilePath, records);
                    break;
                    // ... other cases as needed ...
            }
        }
    }

    static void DisplayHelp()
    {
        Console.Clear();
        Console.WriteLine("  Help Menu:");
        Console.WriteLine("  -----------------------------------------------------");
        Console.WriteLine("  j or Down Arrow: Move selection down by one record.");
        Console.WriteLine("  k or Up Arrow: Move selection up by one record.");
        Console.WriteLine("  Page Down: Move selection down by 20 records.");
        Console.WriteLine("  Page Up: Move selection up by 20 records.");
        Console.WriteLine("  Home: Jump to the first record.");
        Console.WriteLine("  End: Jump to the last record.");
        Console.WriteLine("  g: Jump to a specific row number.");
        Console.WriteLine("  e: Edit the current record.");
        Console.WriteLine("  q: Quit the application.");
        Console.WriteLine("  <space>: toggle checked");
        Console.WriteLine("  h: Display this help menu.");
        Console.WriteLine("  -----------------------------------------------------");
        Console.WriteLine("  Press any key to return to the application...");
        Console.ReadKey(intercept: true); // Wait for a key press before returning
    }


    static void DisplayRecords(List<LabelRecord> records, int selectedIndex)
    {
        Console.Clear();

        // Header titles
        string numberHeader = "#";
        string labelPathHeader = "Label Path";
        string commentHeader = "Comment";
        string checkedHeader = "Checked";

        // Determine the width of each column based on the header and current records
        int numberWidth = records.Count.ToString().Length + 3; // +3 for padding and the dot
        int labelPathWidth = Math.Max(labelPathHeader.Length, records.Max(r => r.LabelPath.Length)) + 2; // +2 for padding
        int commentWidth = Math.Max(commentHeader.Length, records.Max(r => r.Comment?.Length ?? 0)) + 2; // +2 for padding
        int checkedWidth = checkedHeader.Length + 2; // +2 for padding

        // Print header
        Console.WriteLine($"{numberHeader.PadRight(numberWidth)}{labelPathHeader.PadRight(labelPathWidth)}{commentHeader.PadRight(commentWidth)}{checkedHeader.PadRight(checkedWidth)}");
        Console.WriteLine(new string('-', numberWidth + labelPathWidth + commentWidth + checkedWidth));

        int displayRange = 5; // Number of records to display before and after the selected one
        int start = Math.Max(0, selectedIndex - displayRange);
        int end = Math.Min(records.Count - 1, selectedIndex + displayRange);
        Console.ForegroundColor = ConsoleColor.White;
        for (int i = start; i <= end; i++)
        {
            Console.ForegroundColor = ConsoleColor.White;
            if (i == selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.Cyan;
                Console.ForegroundColor = ConsoleColor.Black;
            }

            // Pad each column
            string number = (i + 1).ToString().PadRight(numberWidth);
            string labelPath = records[i].LabelPath.PadRight(labelPathWidth);
            string comment = (records[i].Comment ?? string.Empty).PadRight(commentWidth);
            string checkedStr = records[i].Checked.ToString().PadRight(checkedWidth);

            Console.WriteLine($"{number}{labelPath}{comment}{checkedStr}");

            Console.ResetColor();
        }
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (start > 0)
        {
            Console.WriteLine($"... ({start} records above) ...");
        }

        if (end < records.Count - 1)
        {
            Console.WriteLine($"... ({records.Count - 1 - end} records below) ...");
        }
    }



    static void EditRecord(List<LabelRecord> records, int selectedIndex, string csvFilePath)
    {
        // Get the selected record
        LabelRecord record = records[selectedIndex];

        // Prompt the user to open the file or skip
        Console.WriteLine($"Do you want to open the file '{record.LabelPath}'? (y/n)");
        var key = Console.ReadKey(intercept: true); // 'true' to not display the pressed key
        if (key.KeyChar == 'y' || key.KeyChar == 'Y')
        {
            Process.Start("explorer", record.LabelPath);
        }

        // Prompt for new comment
        Console.WriteLine("Enter new comment (leave blank to keep current):");
        string newComment = Console.ReadLine();
        if (!string.IsNullOrEmpty(newComment))
        {
            record.Comment = newComment;
        }

        // Toggle checked status
        Console.WriteLine("Toggle checked? (current value: " + record.Checked + ") (y/n)");
        key = Console.ReadKey(intercept: true); // 'true' to not display the pressed key
        if (key.KeyChar == 'y' || key.KeyChar == 'Y')
        {
            record.Checked = !record.Checked;
        }

        // Update the CSV file with the new information
        UpdateCsvFile(csvFilePath, records);
    }


    static List<LabelRecord> ReadCsvFile(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
        {
            return new List<LabelRecord>(csv.GetRecords<LabelRecord>());
        }
    }

    static void DisplayRecords(List<LabelRecord> records)
    {
        for (int i = 0; i < records.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {records[i].LabelPath} - Comment: {records[i].Comment} - Checked: {records[i].Checked}");
        }
    }

    static void UpdateCsvFile(string filePath, List<LabelRecord> records)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(records);
        }
    }
}

public class CustomBooleanConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false; // Assume false for empty strings
        }

        // Try to parse the text as a boolean
        if (bool.TryParse(text, out bool result))
        {
            return result;
        }

        // If parsing fails, throw an exception or handle it as needed
        throw new TypeConverterException(this, memberMapData, text, row.Context, $"Text '{text}' could not be converted to Boolean.");
    }
}

public class LabelRecord
{
    [Name("label_path")]
    public string LabelPath { get; set; }

    [Name(" comment")] // Note the leading space
    public string Comment { get; set; }

    [Name(" checked")] // Note the leading space
    [TypeConverter(typeof(CustomBooleanConverter))]
    public bool Checked { get; set; }
}


