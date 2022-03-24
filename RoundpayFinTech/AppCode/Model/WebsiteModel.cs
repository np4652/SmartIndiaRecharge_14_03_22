namespace RoundpayFinTech.AppCode.Model
{
    public class WebsiteModel
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public string DomainName { get; set;}
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Prefix { get; set; }
        public string AppName { get; set; }
        public string AppPackageID { get; set; }
        public string RefferalContent { get; set; }
        public bool IsActive { get; set; }
        public int WID { get; set;}
        public int UserID { get; set;}
        public int IsWebsiteUpdate { get; set; }
        public bool IsWLAPIAllowed { get; set; }
    }
}
