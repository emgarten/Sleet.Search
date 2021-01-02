using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Sleet.Search
{
    public static class SearchUtils
    {
        // Field to search, in rev priority order
        public static readonly string[] SearchFields = new string[] { "description", "title", "id" };

        public static JObject GetSearchResult(List<JObject> entries, int totalHits, JObject jsonLdContext)
        {
            var data = new JArray(entries);
            var result = new JObject(
                new JProperty("index", "sleet-external-search"),
                new JProperty("totalHits", totalHits),
                new JProperty("data", data),
                new JProperty("@context", jsonLdContext));

            return result;
        }

        public static NuGetVersion GetVersion(JObject versionEntry)
        {
            return NuGetVersion.Parse(versionEntry["version"].ToObject<string>());
        }

        public static JObject RemovePrePrelPackages(JObject entry)
        {
            var all = ((JArray)entry["versions"]).Select(e => (JObject)e).ToList();
            var filtered = all.Where(e => !GetVersion(e).IsPrerelease).ToList();
            entry["versions"] = new JArray(filtered);
            entry["version"] = filtered.Select(GetVersion).Max().ToNormalizedString();
            return entry;
        }

        public static bool HasStable(JObject entry)
        {
            var all = ((JArray)entry["versions"]).Select(e => (JObject)e).ToList();
            return all.Any(e => !GetVersion(e).IsPrerelease);
        }

        public static List<JObject> FilterOnPreRel(List<JObject> entries, bool prerelease)
        {
            if (prerelease)
            {
                return entries;
            }

            return entries.Where(HasStable).Select(RemovePrePrelPackages).ToList();
        }

        public static List<JObject> SearchEntries(List<JObject> entries, HashSet<string> terms)
        {
            if (terms.Count > 0)
            {
                var ranked = entries.Select(entry =>
                new SearchEntry()
                {
                    Id = entry["id"].ToObject<string>(),
                    Entry = entry,
                    Score = terms.Sum(e => ScoreEntry(entry, e))
                })
               .Where(e => e.Score > 0)
               .ToArray();

                Array.Sort(ranked, new SearchEntryComparer());

                return ranked.Select(e => e.Entry).ToList();
            }

            // Default to everything
            return entries;
        }

        public static HashSet<string> GetTerms(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new HashSet<string>();
            }

            // Limit to 10 terms
            return new HashSet<string>(query.Split(new char[] { '.', ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10));
        }

        public static double ScoreEntry(JObject entry, string term)
        {
            for (var i = 0; i < SearchFields.Length; i++)
            {
                var s = entry[SearchFields[i]].ToObject<string>();

                if (s.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return (i + 1) * 5.0;
                }
            }

            return 0;
        }

        public static List<JObject> GetEntries(JObject json)
        {
            var dataArray = (JArray)json["data"];
            return dataArray.Select(e => (JObject)e).ToList();
        }

        public static async Task<JObject> ReadJson(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return await JObject.LoadAsync(jsonReader);
            }
        }
    }
}
