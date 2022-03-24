using System;
using System.Collections.Generic;

namespace RoundpayFinTech.Models
{
    public class AppResource
    {
        public string ResourceUrl { get; set; }
        public string FileName { get; set; }
        public string SiteResourceUrl { get; set; }
        public string SiteFileName { get; set; }

        public string PopupResourceUrl { get; set; }
        public string PopupFileName { get; set; }
    }
    public class BannerImage : AppResource
    {
        public DateTime Entrydate { get; set; }
        public bool ShouldSerializeEntrydate() => false;
        public string URL { get; set; }
        public string RefUrl { get; set; }
        public IEnumerable<BannerImage> DbUrl { get; set; }
    }
}
