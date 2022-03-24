namespace RoundpayFinTech.AppCode.Model
{
    public class PSTReportEmp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
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
        public int UserID { get; set; }
        public string User { get; set; }
        public string UserMobile { get; set; }
        public int URoleID { get; set; }
        public decimal PriLM { get; set; }
        public decimal PriLMTD { get; set; }
        public decimal Pri { get; set; }
        public decimal PGrowth { get; set; }
        public decimal SecLM { get; set; }
        public decimal SecLMTD { get; set; }
        public decimal Sec { get; set; }
        public decimal SGrowth { get; set; }
        public decimal TerLM { get; set; }
        public decimal TerLMTD { get; set; }
        public decimal Ter { get; set; }
        public decimal TGrowth { get; set; }
        public int TOutletLM { get; set; }
        public int TOutletLMTD { get; set; }
        public int TOutlet { get; set; }
        public int TNewOutletLM { get; set; }
        public int TNewOutletLMTD { get; set; }
        public int TNewOutlet { get; set; }
        public decimal OGrowth { get; set; }
        public decimal PackageSellLM { get; set; }
        public decimal PackageSellLMTD { get; set; }
        public decimal PackageSell { get; set; }
        public decimal PackageGrowth { get; set; }
        public int AMID { get; set; }
        public string AMMobileNo { get; set; }
        public string AM { get; set; }
        public int AMRoleID { get; set; }        
        public string EntryDate { get; set; }
    }

    public class PSTReportUser
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public decimal PriLastDay { get; set; }
        public decimal PriMonthTillDate { get; set; }
        public decimal PriLastMonthTillDate { get; set; }
        public decimal SecLastDay { get; set; }
        public decimal SecMonthTillDate { get; set; }
        public decimal SecLastMonthTillDate { get; set; }
        public decimal TerLastDay { get; set; }
        public decimal TerMonthTillDate { get; set; }
        public decimal TerLastMonthTillDate { get; set; }
        public decimal PriFTD { get; set; }
        public decimal SecFTD { get; set; }
        public decimal TerFTD { get; set; }
        public string EntryDate { get; set; }

        //Difference  Parameters
        public decimal Pri_LMTD_MTD_Diff { get; set; }
        public decimal Sec_LMTD_MTD_Diff { get; set; }
        public decimal Ter_LMTD_MTD_Diff { get; set; }
        public decimal Pri_LastDay_Current_Diff { get; set; }
        public decimal Sec_LastDay_Current_Diff { get; set; }
        public decimal Ter_LastDay_Current_Diff { get; set; }
    }
}
