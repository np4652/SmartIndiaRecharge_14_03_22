using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class FilterWithOptions
    {
        public int FilterID { get; set; }
        public string FilterName { get; set; }
        public IEnumerable<FilterOption> FilterOption { get; set; }
    }

    public class FilterAndOptions
    {
        public IEnumerable<Filter> Filter { get; set; }
        public IEnumerable<FilterOption> FilterOption { get; set; }
    }
}
