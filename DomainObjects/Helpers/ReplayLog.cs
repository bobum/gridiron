using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DomainObjects.Helpers
{
    public class ReplayLog
    {
        public int Seed { get; set; }
        public List<double> Doubles { get; set; } = new List<double>();
        public List<int> Ints { get; set; } = new List<int>();
        public List<RandomIntRangeEntry> IntRanges { get; set; } = new List<RandomIntRangeEntry>();

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static ReplayLog Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ReplayLog>(json) ?? throw new InvalidOperationException("Failed to deserialize ReplayLog from the provided JSON.");
        }
    }

    public class RandomIntRangeEntry
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Value { get; set; }
    }
}