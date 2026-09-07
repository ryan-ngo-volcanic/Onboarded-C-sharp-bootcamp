using System.Globalization;
using CsvHelper;

namespace WikipediaSearch.Api.Features.KeywordUploads;

public class CsvKeywordParser
{
    public CsvParseResult Parse(string csv)
    {
        try
        {
            return ParseCore(csv);
        }
        catch (CsvHelperException)
        {
            return new CsvParseResult([], ["CSV file is malformed."]);
        }

    }

    private static CsvParseResult ParseCore(string csv)
    {
        using var textReader = new StringReader(csv);
        using var csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture);

        if (!csvReader.Read())
        {
            return new CsvParseResult([], ["Missing required 'keyword' header."]);
        }

        csvReader.ReadHeader();
        var headers = csvReader.HeaderRecord;

        if (headers == null || headers.Length != 1 || !headers[0].Equals("keyword", StringComparison.OrdinalIgnoreCase))
        {
            return new CsvParseResult([], ["Missing required 'keyword' header."]);
        }

        var keywordRows = new List<string>();


        while (csvReader.Read())
        {

            var fieldCount = csvReader.Parser.Count;

            if (fieldCount != 1)
            {
                return new CsvParseResult([], ["CSV rows must contain exactly one keyword column."]);
            }

            var keyword = csvReader.GetField(0);
            if (keyword is not null)
            {
                keywordRows.Add(keyword);
            }


        }

        var keywords = keywordRows
           .Select(keyword => keyword.Trim())
           .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
           .Distinct(StringComparer.OrdinalIgnoreCase)
           .ToList();

        if (keywords.Count == 0)
        {
            return new CsvParseResult([], ["CSV must contain at least one keyword."]);
        }

        if (keywords.Count > 100)
        {
            return new CsvParseResult([], ["CSV cannot contain more than 100 keywords."]);
        }

        if (keywords.Any(x => x.Length > 100))
        {
            return new CsvParseResult([], ["Keywords cannot exceed 100 characters."]);
        }

        return new CsvParseResult(keywords, []);
    }
}
