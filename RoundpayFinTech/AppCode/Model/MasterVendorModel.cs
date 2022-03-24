using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class MasterVendorModel
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public string VendorName { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public string SelectedOperators { get; set; }
        public IEnumerable<OpTypeMaster> OperatorDdl { get; set; }
    }

    public class VendorBindOperators
    {
        public int ID { get; set; }
        public string VendorName { get; set; }
        public List<int> SelectedOperators { get; set; }
        public string SelectOps { get; set; }
        public IEnumerable<OpTypeMaster> OperatorDdl { get; set; }
    }
}
