using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class FilterForEndUser
    {
        public IEnumerable<FilterWithOptions> FilterWithOptions { get; set; }
        public IEnumerable<Brand> Brands { get; set; }
    }
}
