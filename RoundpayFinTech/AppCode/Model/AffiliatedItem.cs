using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class AffiliatedItem
    {
        public int ID { get; set; }
        public int VendorID { get; set; }
        public int OID { get; set; }
        public string VendorName { get; set; }
        public string Operator { get; set; }
        public string Link { get; set; }
        public string ImgUrl { get; set; }
        public string ImageURL { get; set; }
        public string EntryDate { get; set; }
        public bool IsActive { get; set; }
        public int LinkType { get; set; }
        public bool IsImageURL { get; set; }
    }

    public class AffiliateItemModal: AffiliatedItem
    {
        public IEnumerable<OperatorDetail> Operator { get; set; }

       
    }
}
