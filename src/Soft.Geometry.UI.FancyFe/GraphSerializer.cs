using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Soft.Geometry.UI.FancyFe
{
    public static class GraphSerializer
    {
        public static void Save(string path, GraphModel graph)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(graph, options);
            File.WriteAllText(path, json);
        }

        public static GraphModel Load(string path)
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Deserialize<GraphModel>(json, options);
        }

        public static string ToJson(GraphModel graph)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(graph, options);
        }

        public static GraphModel FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Deserialize<GraphModel>(json, options);
        }
    }
}
