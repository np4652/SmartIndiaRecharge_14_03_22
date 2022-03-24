using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class Filter
    {
        public int ID { get; set; }
        public string FilterName { get; set; }
        public bool IsActive { get; set; }
        public bool IsEditable { get; set; }
    }

    public class FilterMapping
    {
        public int SubCategoryId { get; set; }
        public IEnumerable<Filter> filter { get; set; }
    }

    public class FilterOption
    {
        public int ID { get; set; }
        public int FilterID { get; set; }
        public int FilterOptionID { get; set; }
        public string Option { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<UOM> Uoms { get; set; }
        public IEnumerable<Colors> Colors { get; set; }
        public string OptionalID { get; set; }
        public bool IsSelected { get; set; }
    }
}
