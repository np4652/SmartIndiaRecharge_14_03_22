using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class AFItemsForDisplay
    {
        public IEnumerable<AfVendorWithCategories> data { get; set; }
    }

    public class AfVendorWithCategories
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public IEnumerable<AfCategoriesWithItems> CategoryWithItems { get; set; }
    }

    public class AfCategoriesWithItems
    {
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public IEnumerable<AfProducts> Items { get; set; }
    }

    public class AfProducts
    {
        public int ID { get; set; }
        public int VendorID { get; set; }        
        public string Link { get; set; }
        public string ImgUrl { get; set; }
        public int LinkType { get; set; }
    }
}
