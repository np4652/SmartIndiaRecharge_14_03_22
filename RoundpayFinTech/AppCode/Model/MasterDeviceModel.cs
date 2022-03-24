using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class MasterDeviceModel
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ModelName { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public IEnumerable<MasterVendorModel> VendorDdl { get; set; }
        public IEnumerable<OpTypeMaster> OperatorDdl { get; set; }
    }
}
