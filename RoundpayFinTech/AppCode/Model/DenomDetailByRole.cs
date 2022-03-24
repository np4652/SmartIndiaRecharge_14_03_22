using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class DenomDetailByRole
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public int ID { get; set; }
        public int DenomID { get; set; }
        public int DenomRangeID { get; set; }
        public int SlabID { get; set; }
        public int OID { get; set; }
        public decimal Comm { get; set; }
        public int AmtType { get; set; }
        public string ModifyDate { get; set; }
        public bool IsAdminDefined { get; set; }
    }

    public class DenomDetailReq : CommonReq
    {
        public DenomDetailByRole Detail { get; set; }
    }
}
