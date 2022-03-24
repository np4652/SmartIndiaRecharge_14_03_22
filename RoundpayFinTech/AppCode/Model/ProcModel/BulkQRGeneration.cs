using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class QRGenData
    {
        public int RefID { get; set; }
        public string TransactionID { get; set; }
        public string BankRefID { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public int AssignedTo { get; set; }
        public string _AssignedTo { get; set; }
        public string AssignedDate { get; set; }

    }

    public class QRGenerationReq
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public List<QRGenData> QRGenData { get; set; }
        public PegeSetting PegeSetting { get; set; }

    }

    public class QRFilter
    {
        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public int FilterID { get; set; }
        public string FilterText { get; set; }
    }
}