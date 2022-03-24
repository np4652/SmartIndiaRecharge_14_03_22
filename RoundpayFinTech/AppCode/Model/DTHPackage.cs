
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.Model
{
    public class DTHPackage
    {
        public int ID { get; set; }
        public int OID { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public int OPTypeID { get; set; }
        public string PackageName { get; set; }
        public int PackageMRP { get; set; }
        public int BookingAmount { get; set; }
        public int FRC { get; set; }
        public int ChannelCount { get; set; }
        public int Validity { get; set; }
        public string Description { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public string SPKey { get; set; }
        public string BusinessModel { get; set; }
        public decimal Comm { get; set; }
    }
    public class DTHPackageReq: DTHPackage
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public DataTable PackageTable { get; set; }
    }
    public class DTHPackageRes
    {
        public DTHPackage package { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
        public List<OperatorDetail> Operators { get; set; }
    }
}
