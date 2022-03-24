using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class TargetModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public int SlabID { get; set; }
        public int Target { get; set; }
        public decimal Comm { get; set; }
        public int AmtType { get; set; }
        public bool IsEarned { get; set; }
        public bool IsGift { get; set; }
        public bool IsAdminDefined { get; set; }
        public int OID { get; set; }
        public string ModifyDate { get; set; }
        public string OpTypeName { get; set; }
        public List<OperatorDetail> Operator { get; set; }
        public string OpName { get; set; }
        public string ServiceName { get; set; }
        public int TargetTypeID { get; set; }
        public decimal HikePer { get; set; }
        public bool IsHikeOnEarned { get; set; }
        public string ImgaePath  { get; set; }
    }

    public class TargetModelReq : CommonReq
    {
        public TargetModel Detail { get; set; }
    }

    public class _TargetModel
    {
        public int TargetType { get; set; }
        public List<TargetModel> TargetModelList { get; set; }
    }
    public class TargetAchieved : TargetModel
    {
        public int SID { get; set; }
        public string Service { get; set; }
        public decimal Target { get; set; }
        public decimal TargetTillDate { get; set; }
        public int TodaySale { get; set; }

    }



}
