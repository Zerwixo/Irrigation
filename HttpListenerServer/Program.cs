using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Server;

class Program
{
    static string? GetQueryValue(NameValueCollection parsed, string key)
    {
        return parsed[key] ?? parsed[key.ToLowerInvariant()];
    }

    static Data parseArgs(NameValueCollection parsed)
    {
        var data = new Data();

        if (int.TryParse(GetQueryValue(parsed, "Id"), out int id)) data.Id = id;
        data.Date = GetQueryValue(parsed, "Date") ?? "";
        data.Parcelle = GetQueryValue(parsed, "Parcelle") ?? "";
        data.Appareil = GetQueryValue(parsed, "Appareil") ?? "";
        if (int.TryParse(GetQueryValue(parsed, "M3d"), out int m3d)) data.M3d = m3d;
        if (int.TryParse(GetQueryValue(parsed, "M3a"), out int m3a)) data.M3a = m3a;
        if (int.TryParse(GetQueryValue(parsed, "Consomation"), out int conso)) data.Consomation = conso;
        data.Reseau = GetQueryValue(parsed, "Reseau") ?? "";
        data.Commentaire = GetQueryValue(parsed, "Commentaire") ?? "";

        return data;
    }

    static void Main(string[] args)
    {
        AppState.DataList = new List<Data>();
        Console.WriteLine("Starting server...");
        using var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");

        listener.Start();

        while(true)
        {
            HttpListenerContext ctx = listener.GetContext();
            using HttpListenerResponse resp = ctx.Response;
            if (ctx.Request.HttpMethod == "POST")
            {
                Console.WriteLine("Received POST request");
                var data = parseArgs(ctx.Request.QueryString);
                if (data != null)
                {
                    // Ensure server assigns a unique id when client does not provide one.
                    if (data.Id <= 0)
                    {
                        data.Id = (AppState.DataList?.Count ?? 0) + 1;
                    }
                    AppState.DataList?.Add(data);
                }
                else
                {
                    Console.WriteLine("Failed to parse data from request");
                }
            }
            else if (ctx.Request.HttpMethod == "PUT")
            {
                Console.WriteLine("Received PUT request");
                var updated = parseArgs(ctx.Request.QueryString);
                var existing = AppState.DataList?.FirstOrDefault(d => d.Id == updated.Id);

                if (existing is not null)
                {
                    existing.Date = updated.Date;
                    existing.Parcelle = updated.Parcelle;
                    existing.Appareil = updated.Appareil;
                    existing.M3d = updated.M3d;
                    existing.M3a = updated.M3a;
                    existing.Consomation = updated.Consomation;
                    existing.Reseau = updated.Reseau;
                    existing.Commentaire = updated.Commentaire;
                }
                else
                {
                    resp.StatusCode = (int)HttpStatusCode.NotFound;
                    resp.StatusDescription = "Data not found";
                    continue;
                }
            }
            else if (ctx.Request.HttpMethod == "DELETE")
            {
                Console.WriteLine("Received DELETE request");
                if (!int.TryParse(GetQueryValue(ctx.Request.QueryString, "Id"), out int idToDelete))
                {
                    resp.StatusCode = (int)HttpStatusCode.BadRequest;
                    resp.StatusDescription = "Missing or invalid id";
                    continue;
                }

                var existing = AppState.DataList?.FirstOrDefault(d => d.Id == idToDelete);
                if (existing is null)
                {
                    resp.StatusCode = (int)HttpStatusCode.NotFound;
                    resp.StatusDescription = "Data not found";
                    continue;
                }

                AppState.DataList?.Remove(existing);
            }
            else
            {
                Console.WriteLine("Received GET request");
                var payload = JsonSerializer.Serialize(AppState.DataList ?? new List<Data>());
                resp.ContentType = "application/json; charset=utf-8";
                var buffer = Encoding.UTF8.GetBytes(payload);
                ctx.Response.OutputStream.Write(buffer);
                
            }

            resp.StatusCode = (int) HttpStatusCode.OK;
            resp.StatusDescription = "Status OK";
        }
    }
}