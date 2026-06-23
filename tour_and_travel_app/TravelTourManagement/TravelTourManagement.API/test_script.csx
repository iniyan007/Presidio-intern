#r "nuget: Npgsql, 8.0.2"

using System;
using Npgsql;
using System.Text.RegularExpressions;

var connString = "Host=localhost;Database=traveldb;Username=postgres;Password=postgres";
using var conn = new NpgsqlConnection(connString);
conn.Open();

using var cmd = new NpgsqlCommand("SELECT id, title, packager_id FROM packages", conn);
using var reader = cmd.ExecuteReader();

var normalize = (string? s) => s == null ? "" : Regex.Replace(s, @"\s+", " ").Trim();
var requestTitle = "Romantic Bali Honeymoon Retreat";
var packagerId = Guid.Parse("00d9f102-a979-4e40-9616-911f339824aa");

Console.WriteLine($"Request Title: '{requestTitle}'");
while(reader.Read())
{
    var id = reader.GetGuid(0);
    var title = reader.GetString(1);
    var pId = reader.GetGuid(2);
    
    if (pId == packagerId)
    {
        var normTitle = normalize(title);
        var normReq = normalize(requestTitle);
        var match = string.Equals(normTitle, normReq, StringComparison.OrdinalIgnoreCase);
        Console.WriteLine($"ID: {id}, DbTitle: '{title}', Match: {match}");
    }
}
