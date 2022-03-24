using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class CreateLead
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public int Id { get; set; }
        public string FromMail { get; set; }
        public string ToMail { get; set; }
        public string Name { get; set; }
        public string UserEmail { get; set; }
        public string MobileNo { get; set; }
        public string Message { get; set; }
        public int RequestModeID { get; set; }
        public string RequestIP { get; set; }
        public string Entrydate { get; set; }
        public string ISsent { get; set; }
        public string Body { get; set; }
        public string RequestPage { get; set; }
        public string RequestStatus { get; set; }
        public string CustomerCareID { get; set; }
        public string Modifyby { get; set; }
        public string ModifyDate { get; set; }
        public string Remarks { get; set; }
        public string IsDeleted { get; set; }
        public string NextFollowupDate { get; set; }
        public string AssignDate { get; set; }

    }
}
