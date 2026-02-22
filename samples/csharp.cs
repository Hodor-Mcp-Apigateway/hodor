// Hodor API - C# sample (Linux, Mac, Windows)
// Run: dotnet script csharp.cs  (requires dotnet-script)
// Or copy to a Console app and add: dotnet add package System.Net.Http.Json

using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = Environment.GetEnvironmentVariable("HODOR_URL") ?? "http://localhost:8080";
using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine("=== Hodor MCP Gateway - C# sample ===");
Console.WriteLine($"Base URL: {baseUrl}\n");

Console.WriteLine("1. Health: " + JsonSerializer.Serialize(await client.GetFromJsonAsync<JsonElement>("/health")));
Console.WriteLine("2. Ready: " + JsonSerializer.Serialize(await client.GetFromJsonAsync<JsonElement>("/ready")));
Console.WriteLine("3. Tools: " + (await client.GetFromJsonAsync<JsonElement>("/api/tools")).GetRawText().Substring(0, 200) + "...");
