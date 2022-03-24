namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class OutletProcModal
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public int KYCStatus { get; set; }
        public string OutletName { get; set; }
        public bool IsOutsider { get; set; }
        public OutletRequest data { get; set; }
    }
}
