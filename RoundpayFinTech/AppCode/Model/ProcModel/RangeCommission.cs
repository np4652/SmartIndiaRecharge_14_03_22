using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class RangeCommission : OperatorDetail
    {
        public int SlabID { get; set; }
        public int SlabDetailID { get; set; }
        public decimal Comm { get; set; }
        public int CommType { get; set; }
        public int AmtType { get; set; }
        public int RoleID { get; set; }
        public int DMRModelID { get; set; }
        public decimal MaxComm { get; set; }
        public decimal FixedCharge { get; set; }
        public decimal RComm { get; set; }
        public int RCommType { get; set; }
        public int RAmtType { get; set; }
        public decimal RMaxComm { get; set; }
        public decimal RFixedCharge { get; set; }
    }
    public class RangeDetailModel
    {
        public int SlabID { get; set; }
        public bool IsAdminDefined { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsChannel { get; set; }
        public List<RangeCommission> SlabDetails { get; set; }
        public List<RangeCommission> ParentSlabDetails { get; set; }
        public List<RoleMaster> Roles { get; set; }
        public List<OperatorDetail> Operators { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
        public SelectList DMRModelSelect { get; set; }
        public int OpTypeID { get; set; }
    }
    public class RangeCommissionReq : CommonReq
    {
        public RangeCommission Commission { get; set; }
    }
}
