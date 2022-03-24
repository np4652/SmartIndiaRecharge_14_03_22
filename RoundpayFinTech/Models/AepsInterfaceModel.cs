using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class AepsInterfaceModel
    {
        public int OID { get; set; }
        public int AEPSInterfaceType { get; set; }
        public PartnerDetailsResp PartnerDetail { get; set; }
        public OutletsOfUsersList outlet { get; set; }
        public SelectList BankList { get; set; }
        public List<BankMaster> BankDetails{ get; set; }
        public IEnumerable<OperatorDetail> Operators { get; set; }        
    }


    public class BankDetail
    {
        public int BankID { get; set; }
        public int IIN { get; set; }
        public string BankName { get; set; }
    }
}
