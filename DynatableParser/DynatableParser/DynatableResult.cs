using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynatableParser
{
    public class DynatableResult<T>
    {
        public IQueryable<T> Records { get; set; }
        public int QueryRecordCount { get; set; }
        public int TotalRecordCount { get; set; }
    }
}