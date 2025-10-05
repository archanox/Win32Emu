using System.Text.Json;
using System.Text.Json.Nodes;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: enrich-from-wikidata <input.json> <output.json>");
    return 1;
}

string inputFile = args[0];
string outputFile = args[1];

// Read input stub
JsonNode? stub;
try
{
    var jsonText = await File.ReadAllTextAsync(inputFile);
    stub = JsonNode.Parse(jsonText);
    if (stub == null)
    {
        Console.Error.WriteLine("Error: Failed to parse input JSON");
        return 1;
    }
}
catch (Exception e)
{
    Console.Error.WriteLine($"Error reading input file: {e.Message}");
    return 1;
}

// Enrich with Wikidata
var enricher = new WikidataEnricher();
var enrichedStub = await enricher.EnrichStubAsync(stub);

// Write output
try
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    var jsonText = enrichedStub.ToJsonString(options);
    await File.WriteAllTextAsync(outputFile, jsonText);
    Console.Error.WriteLine($"\nEnriched stub written to {outputFile}");
}
catch (Exception e)
{
    Console.Error.WriteLine($"Error writing output file: {e.Message}");
    return 1;
}

return 0;

class WikidataEnricher
{
    private const string BaseUrl = "https://www.wikidata.org/w/api.php";
    private const string UserAgent = "Win32Emu-GameDB-Bot/1.0 (https://github.com/archanox/Win32Emu)";

    private static readonly Dictionary<string, string> LanguageCodes = new()
    {
        { "Q1860", "en" },   // English
        { "Q150", "fr" },    // French
        { "Q188", "de" },    // German
        { "Q1321", "es" },   // Spanish
        { "Q652", "it" },    // Italian
        { "Q5146", "pt" },   // Portuguese
        { "Q7411", "nl" },   // Dutch
        { "Q9027", "sv" },   // Swedish
        { "Q9056", "cs" },   // Czech
        { "Q9058", "pl" },   // Polish
        { "Q9292", "az" },   // Azerbaijani
        { "Q8748", "zh" },   // Chinese
        { "Q5287", "ja" },   // Japanese
        { "Q9176", "ko" },   // Korean
        { "Q7737", "ru" }    // Russian
    };

    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, JsonNode?> _entityCache = new();

    public WikidataEnricher()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public async Task<JsonNode> EnrichStubAsync(JsonNode stub)
    {
        var wikidataKey = stub["WikidataKey"]?.GetValue<string>();
        if (string.IsNullOrEmpty(wikidataKey))
        {
            Console.Error.WriteLine("No WikidataKey found in stub, skipping enrichment");
            return stub;
        }

        Console.Error.WriteLine($"Fetching data from Wikidata for {wikidataKey}...");
        var entity = await GetEntityAsync(wikidataKey);
        if (entity == null)
        {
            Console.Error.WriteLine($"Failed to fetch Wikidata entity {wikidataKey}");
            return stub;
        }

        var enriched = JsonNode.Parse(stub.ToJsonString())!;
        var claims = entity["claims"]?.AsObject();

        // Enrich Title if not provided
        if (string.IsNullOrEmpty(enriched["Title"]?.GetValue<string>()))
        {
            var labels = entity["labels"]?.AsObject();
            var enLabel = labels?["en"]?["value"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(enLabel))
            {
                enriched["Title"] = enLabel;
                Console.Error.WriteLine($"  ✓ Title: {enLabel}");
            }
        }

        // Enrich Description if not provided
        if (string.IsNullOrEmpty(enriched["Description"]?.GetValue<string>()))
        {
            var descriptions = entity["descriptions"]?.AsObject();
            var enDesc = descriptions?["en"]?["value"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(enDesc))
            {
                enriched["Description"] = enDesc;
                Console.Error.WriteLine($"  ✓ Description: {enDesc}");
            }
        }

        // Enrich ReleaseDate if not provided
        if (string.IsNullOrEmpty(enriched["ReleaseDate"]?.GetValue<string>()) && claims != null)
        {
            var p577 = claims["P577"]?.AsArray();
            if (p577 != null && p577.Count > 0)
            {
                var claim = p577[0];
                var datavalue = claim?["mainsnak"]?["datavalue"];
                if (datavalue != null)
                {
                    var dateValue = datavalue["value"]?.AsObject();
                    if (dateValue != null)
                    {
                        var parsedDate = ParseDate(dateValue);
                        if (!string.IsNullOrEmpty(parsedDate))
                        {
                            enriched["ReleaseDate"] = parsedDate;
                            Console.Error.WriteLine($"  ✓ ReleaseDate: {parsedDate}");
                        }
                    }
                }
            }
        }

        // Enrich Languages if not provided or empty
        var languages = enriched["Languages"]?.AsArray();
        if (languages == null || languages.Count == 0)
        {
            var languageIds = ExtractClaimValues(claims, "P407");
            var langCodes = new List<string>();
            foreach (var langId in languageIds)
            {
                if (LanguageCodes.TryGetValue(langId, out var code))
                {
                    langCodes.Add(code);
                }
            }
            if (langCodes.Count > 0)
            {
                enriched["Languages"] = new JsonArray(langCodes.Select(c => JsonValue.Create(c)).ToArray());
                Console.Error.WriteLine($"  ✓ Languages: {string.Join(", ", langCodes)}");
            }
        }

        // Add Wikidata URL to ExternalUrls if not present
        var externalUrls = enriched["ExternalUrls"]?.AsObject();
        if (externalUrls == null)
        {
            externalUrls = new JsonObject();
            enriched["ExternalUrls"] = externalUrls;
        }
        if (!externalUrls.ContainsKey("Wikidata"))
        {
            externalUrls["Wikidata"] = $"https://www.wikidata.org/wiki/{wikidataKey}";
            Console.Error.WriteLine("  ✓ Added Wikidata URL");
        }

        // Log genres, developers, publishers (not auto-mapped)
        var genres = ExtractClaimValues(claims, "P136");
        if (genres.Count > 0)
        {
            var genreLabels = new List<string>();
            foreach (var genreId in genres)
            {
                var label = await GetLabelAsync(genreId);
                if (!string.IsNullOrEmpty(label))
                {
                    genreLabels.Add(label);
                }
            }
            if (genreLabels.Count > 0)
            {
                Console.Error.WriteLine($"  ℹ Genres found (not auto-mapped): {string.Join(", ", genreLabels)}");
            }
        }

        var developers = ExtractClaimValues(claims, "P178");
        if (developers.Count > 0)
        {
            var devLabels = new List<string>();
            foreach (var devId in developers)
            {
                var label = await GetLabelAsync(devId);
                if (!string.IsNullOrEmpty(label))
                {
                    devLabels.Add(label);
                }
            }
            if (devLabels.Count > 0)
            {
                Console.Error.WriteLine($"  ℹ Developers found (not auto-mapped): {string.Join(", ", devLabels)}");
            }
        }

        var publishers = ExtractClaimValues(claims, "P123");
        if (publishers.Count > 0)
        {
            var pubLabels = new List<string>();
            foreach (var pubId in publishers)
            {
                var label = await GetLabelAsync(pubId);
                if (!string.IsNullOrEmpty(label))
                {
                    pubLabels.Add(label);
                }
            }
            if (pubLabels.Count > 0)
            {
                Console.Error.WriteLine($"  ℹ Publishers found (not auto-mapped): {string.Join(", ", pubLabels)}");
            }
        }

        return enriched;
    }

    private async Task<JsonNode?> GetEntityAsync(string entityId)
    {
        if (_entityCache.TryGetValue(entityId, out var cached))
        {
            return cached;
        }

        try
        {
            var url = $"{BaseUrl}?action=wbgetentities&ids={entityId}&format=json&languages=en";
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonNode.Parse(response);
            var entity = data?["entities"]?[entityId];
            _entityCache[entityId] = entity;
            return entity;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Warning: Failed to fetch entity {entityId}: {e.Message}");
            return null;
        }
    }

    private async Task<string?> GetLabelAsync(string entityId)
    {
        var entity = await GetEntityAsync(entityId);
        return entity?["labels"]?["en"]?["value"]?.GetValue<string>();
    }

    private static List<string> ExtractClaimValues(JsonObject? claims, string propertyId)
    {
        var values = new List<string>();
        if (claims == null || !claims.ContainsKey(propertyId))
        {
            return values;
        }

        var claimsArray = claims[propertyId]?.AsArray();
        if (claimsArray == null)
        {
            return values;
        }

        foreach (var claim in claimsArray)
        {
            var datavalue = claim?["mainsnak"]?["datavalue"];
            if (datavalue != null && datavalue["type"]?.GetValue<string>() == "wikibase-entityid")
            {
                var entityId = datavalue["value"]?["id"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(entityId))
                {
                    values.Add(entityId);
                }
            }
        }

        return values;
    }

    private static string? ParseDate(JsonObject dateValue)
    {
        try
        {
            var timeStr = dateValue["time"]?.GetValue<string>();
            if (string.IsNullOrEmpty(timeStr))
            {
                return null;
            }

            // Wikidata format: +1997-01-01T00:00:00Z
            timeStr = timeStr.TrimStart('+');
            var precision = dateValue["precision"]?.GetValue<int>() ?? 11;

            if (!DateTime.TryParse(timeStr, out var dt))
            {
                return null;
            }

            // Format based on precision
            return precision switch
            {
                9 => dt.ToString("yyyy-01-01T00:00:00"),  // year
                10 => dt.ToString("yyyy-MM-01T00:00:00"), // month
                _ => dt.ToString("yyyy-MM-ddTHH:mm:ss")   // day or more precise
            };
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Warning: Failed to parse date {dateValue}: {e.Message}");
            return null;
        }
    }
}
