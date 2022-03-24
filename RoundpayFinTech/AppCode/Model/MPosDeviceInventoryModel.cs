using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class MPosDeviceInventoryModel
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int DeviceModelId { get; set; }
        public string DeviceModelName { get; set; }
        public string DeviceSerial { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string AssignedDate { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public string MappedDate { get; set; }
        public bool IsActive { get; set; }
        public bool DeAssign { get; set; }
        public string Browser { get; set; }
        public string IPAddress { get; set; }
        public IEnumerable<MasterVendorModel> VendorDdl { get; set; }
    }

    //public class VendorBindOperators
    //{
    //    public int ID { get; set; }
    //    public string VendorName { get; set; }
    //    public List<int> SelectedOperators { get; set; }
    //    public string SelectOps { get; set; }
    //    public IEnumerable<OpTypeMaster> OperatorDdl { get; set; }
    //}
}
