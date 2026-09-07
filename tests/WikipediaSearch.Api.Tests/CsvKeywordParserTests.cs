using WikipediaSearch.Api.Features.KeywordUploads;

namespace WikipediaSearch.Api.Tests;

public class CsvKeywordParserTests
{
    [Fact]
    public void Parse_WithOneValidKeyword_ReturnsKeyword()
    {
        const string csv = "keyword\napple";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.Single(result.Keywords);
        Assert.Equal("apple", result.Keywords[0]);
        Assert.Empty(result.Errors);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Parse_WithRemoveWhitespace_ReturnsKeyword()
    {
        const string csv = "keyword\n apple ";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.Single(result.Keywords);
        Assert.Equal("apple", result.Keywords[0]);
        Assert.Empty(result.Errors);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Parse_WithDuplicateKeywords_ReturnsUniqueKeywords()
    {
        const string csv = "keyword\napple\napple\nbanana";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.Equal(2, result.Keywords.Count);
        Assert.Equal("apple", result.Keywords[0]);
        Assert.Equal("banana", result.Keywords[1]);
        Assert.Empty(result.Errors);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Parse_WithOnlyHeader_ReturnsValidationError()
    {
        const string csv = "keyword";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.Empty(result.Keywords);
        Assert.Contains("CSV must contain at least one keyword.", result.Errors);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Parse_WithMultipleKeywords_ReturnsAllKeywords()
    {
        const string csv = "keyword\napple\nbanana\ncherry";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.Equal(3, result.Keywords.Count);
        Assert.Equal("apple", result.Keywords[0]);
        Assert.Equal("banana", result.Keywords[1]);
        Assert.Equal("cherry", result.Keywords[2]);
    }

    [Fact]
    public void Parse_WithBlankRows_IgnoresBlankRows()
    {
        const string csv = "keyword\napple\n  \nbanana\ncherry\ndate\nelderberry";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Console.WriteLine($"Result: {result}");

        Assert.Equal(5, result.Keywords.Count);
        Assert.Equal("apple", result.Keywords[0]);
        Assert.Equal("banana", result.Keywords[1]);
        Assert.Equal("cherry", result.Keywords[2]);
        Assert.Equal("date", result.Keywords[3]);
        Assert.Equal("elderberry", result.Keywords[4]);
    }

    [Fact]
    public void Parse_WithCaseInsensitiveDuplicates_PreservesFirstKeyword()
    {
        const string csv = "keyword\nApple\napple\nAPPLE";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.Single(result.Keywords);
        Assert.Equal("Apple", result.Keywords[0]);
        Assert.Empty(result.Errors);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Parse_WithoutKeywordHeader_ReturnsValidationError()
    {
        const string csv = "apple\nbanana";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains("Missing required 'keyword' header.", result.Errors);
    }

    [Fact]
    public void Parse_WithEmptyCsv_ReturnsMissingHeaderError()
    {
        const string csv = "";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains("Missing required 'keyword' header.", result.Errors);
    }

    [Fact]
    public void Parse_WithHeaderStartingWithKeyword_ReturnsValidationError()
    {
        const string csv = "keywordHeader\napple\nbanana";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains("Missing required 'keyword' header.", result.Errors);
    }

    [Fact]
    public void Parse_WithWindowsLineEndings_ReturnsKeywords()
    {
        const string csv = "keyword\r\napple\r\nbanana";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(2, result.Keywords.Count);
        Assert.Equal("apple", result.Keywords[0]);
        Assert.Equal("banana", result.Keywords[1]);
    }

    [Fact]
    public void Parse_WithOnlyBlankKeywordRows_ReturnsValidationError()
    {
        const string csv = "keyword\n  \n   \n\t";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains("CSV must contain at least one keyword.", result.Errors);
    }

    [Fact]
    public void Parse_WithExactlyOneHundredKeywords_ReturnsAllKeywords()
    {
        var keywordRows = Enumerable.Range(1, 100)
            .Select(number => $"keyword-{number}");
        var csv = "keyword\n" + string.Join("\n", keywordRows);
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(100, result.Keywords.Count);
    }

    [Fact]
    public void Parse_WithMoreThanOneHundredKeywords_ReturnsValidationError()
    {
        var keywordRows = Enumerable.Range(1, 101)
            .Select(number => $"keyword-{number}");
        var csv = "keyword\n" + string.Join("\n", keywordRows);
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains(
            "CSV cannot contain more than 100 keywords.",
            result.Errors);
    }

    [Fact]
    public void Parse_WithOneHundredUniqueKeywordsAndOneDuplicate_IsValid()
    {
        var keywordRows = Enumerable.Range(1, 100)
            .Select(number => $"keyword-{number}");
        var csv = "keyword\n" + string.Join("\n", keywordRows) + "\n" + "Keyword-1";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(100, result.Keywords.Count);
    }

    [Fact]
    public void Parse_WithOneHundredCharacterKeyword_IsValid()
    {
        var keyword = new string('a', 100);
        var csv = $"keyword\n{keyword}";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Single(result.Keywords);
        Assert.Equal(100, result.Keywords[0].Length);
    }

    [Fact]
    public void Parse_WithKeywordLongerThanOneHundredCharacters_ReturnsValidationError()
    {
        var keyword = new string('a', 101);
        var csv = $"keyword\n{keyword}";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains(
            "Keywords cannot exceed 100 characters.",
            result.Errors);
    }

    [Fact]
    public void Parse_WithWhitespaceAroundOneHundredCharacterKeyword_IsValid()
    {
        var keyword = new string('a', 100);
        var csv = $"keyword\n  {keyword}  ";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Single(result.Keywords);
        Assert.Equal(keyword, result.Keywords[0]);
        Assert.Equal(100, result.Keywords[0].Length);
    }

    [Fact]
    public void Parse_WithQuotedKeywordContainingComma_ReturnsKeywordWithoutQuotes()
    {
        var csv = "keyword\n\"hello,world\"";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Single(result.Keywords);
        Assert.Equal("hello,world", result.Keywords[0]);
    }

    [Fact]
    public void Parse_WithQuotedMultilineKeyword_ReturnsOneKeyword()
    {
        const string csv = "keyword\n\"hello\nworld\"";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Single(result.Keywords);
        Assert.Equal("hello\nworld", result.Keywords[0]);
    }

    [Fact]
    public void Parse_WithExtraDataColumn_ReturnsValidationError()
    {
        const string csv = "keyword\napple,banana";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains(
            "CSV rows must contain exactly one keyword column.",
            result.Errors);
    }

    [Fact]
    public void Parse_WithMalformedCsv_ReturnsValidationError()
    {
        const string csv = "keyword\nhello\"world";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Contains("CSV file is malformed.", result.Errors);
    }

    [Fact]
    public void Parse_WithEscapedQuote_ReturnsKeywordContainingQuote()
    {
        const string csv = "keyword\n\"say \"\"hello\"\"\"";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Single(result.Keywords);
        Assert.Equal("say \"hello\"", result.Keywords[0]);
    }

    [Fact]
    public void Parse_WithMalformedHeader_ReturnsValidationError()
    {
        const string csv = "key\"word\napple";
        var parser = new CsvKeywordParser();

        var result = parser.Parse(csv);

        Assert.False(result.IsValid);
        Assert.Empty(result.Keywords);
        Assert.Single(result.Errors);
        Assert.Equal("CSV file is malformed.", result.Errors[0]);
    }
}
