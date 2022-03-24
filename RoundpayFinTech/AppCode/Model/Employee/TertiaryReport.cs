
namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class TertiaryReport
    {
        public int Statuscode { get; set; }
        public string Msg{ get; set; }
        public string ID { get; set; }
        public int ServiceID { get; set; }
        public int OPTypeID { get; set; }
        public int UserID { get; set; }
        public string User { get; set; }
        public string UserMobile { get; set; }
        public int URoleID { get; set; }
        public string URole { get; set; }
        public string AM { get; set; }
        public int AMRoleID { get; set; }
        public string AMRole { get; set; }
        public int SHID { get; set; }
        public string SHDetail { get; set; }
        public int CHID { get; set; }
        public string CDetail { get; set; }
        public int ZID { get; set; }
        public string ZDetail { get; set; }
        public int AID { get; set; }
        public string ADetail { get; set; }
        public int TSMID { get; set; }
        public string TSMDetail { get; set; }
        //Prepaid
        public string LMPrepaid { get; set; }
        public string LMTDPrepaid { get; set; }
        public string MTDPrepaid { get; set; }
        public string TOLMPrepaid { get; set; }
        public string TOLMTDPrepaid { get; set; }
        public string TOMTDPrepaid { get; set; }
        //DMT
        public string LMDMT { get; set; }
        public string LMTDDMT { get; set; }
        public string MTDDMT { get; set; }
        public string TOLMDMT { get; set; }
        public string TOLMTDDMT { get; set; }
        public string TOMTDDMT { get; set; }
        //BBPS
        public string LMBBPS { get; set; }
        public string LMTDBBPS { get; set; }
        public string MTDBBPS { get; set; }
        public string TOLMBBPS { get; set; }
        public string TOLMTDBBPS { get; set; }
        public string TOMTDBBPS { get; set; }
        //AEPS
        public string LMAEPS { get; set; }
        public string LMTDAEPS { get; set; }
        public string MTDAEPS { get; set; }
        public string TOLMAEPS { get; set; }
        public string TOLMTDAEPS { get; set; }
        public string TOMTDAEPS { get; set; }
        public string EntryDate { get; set; }
        public string TransactionDate { get; set; }

    }
}
