using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Sleet.Search
{
    public class SearchEntry
    {
        public JObject Entry { get; set; }

        public double Score { get; set; }

        public string Id { get; set; }
    }

    public class SearchEntryComparer : IComparer<SearchEntry>
    {
        public int Compare([AllowNull] SearchEntry x, [AllowNull] SearchEntry y)
        {
            if (x.Score > y.Score)
            {
                return -1;
            }

            if (y.Score > x.Score)
            {
                return 1;
            }

            return StringComparer.OrdinalIgnoreCase.Compare(x.Id, y.Id);
        }
    }
}
