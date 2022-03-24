using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    

    public class UserCallMeModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public int StatusID { get; set; }
        public string Status { get; set; }
        public string CallHistory { get; set; }
        public string EntryDate { get; set; }
    }

        public class CallMeHistoryModel
    {
        public int ID { get; set; }
        public int CallMeID { get; set; }
        public string CallHistory { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public int ModifyBy { get; set; }
        public string ModifiedDate { get; set; }
    }
}
