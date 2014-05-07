using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Diagnostics;


namespace DynatableParser
{
    public class DynatableParser<T> where T : class
    {
        public DynatableResult<T> Result { set; get; }

        public DynatableParser(IEnumerable<KeyValuePair<string,string>> request, IQueryable<T> data)
        {
            DynatableResult<T> result = new DynatableResult<T>();
            result.TotalRecordCount = data.Count();

            var offsetParameter = request.Where(x => x.Key.StartsWith("offset")).First().Value;
            var perPageParameter = request.Where(x => x.Key.StartsWith("perPage")).First().Value;

            var filterParams = request.Where(x => x.Key.StartsWith("queries")).Select(x=> x.Value);
            Filter<T> filter = new Filter<T>(data, filterParams.FirstOrDefault());
            result.QueryRecordCount = filter.FilteredData.Count();

            var sortParameters = request.Select(ParseSortParameters).ToList().Where(x=> x.Key != null);
            Sorter<T> sorter = new Sorter<T>(filter.FilteredData, sortParameters);
            
            var records = sorter.SortedData.Skip(Int32.Parse(offsetParameter)).Take(Int32.Parse(perPageParameter));
            result.Records = records;
            
            Result = result;
        }

        /// <summary>
        /// Extracts the property name from the patterns sorts.propertyName or sorts[propertyName]
        /// </summary>
        private KeyValuePair<string,string> ParseSortParameters (KeyValuePair<string,string> input)
        {
            String extractedPropertyName = null;
            string[] patterns = new string[] { @"sorts\[(.*)\]", @"sorts\.(.*)" };

            if (input.Key.StartsWith("sorts"))
            {
                foreach (var pattern in patterns)
                {
                    Regex r = new Regex(pattern);
                    Match matchResult = r.Match(input.Key);
                    if (matchResult.Success)
                    {
                        extractedPropertyName = matchResult.Groups[1].Value;
                        break;
                    }
                }
            }

            return new KeyValuePair<string, string>(extractedPropertyName, input.Value);
        }
    }


    
}