namespace RoundpayFinTech.AppCode.Model
{
    public class OutletAPIWiseDetail: APIAppSetting
    {
        public int ID { get; set; }
        public int OutletID { get; set; }
        public int APIID { get; set; }
        public string ApiOpcode { get; set; }

        public string Scode { get; set; }

        public int APIOutletID { get; set; }
        public int VerifyStatus { get; set; }
        public int DocVerifyStatus { get; set; }
       
        public string AEPSID { get; set; }
        public int AEPSStatus { get; set; }

        public string PANID { get; set; }
        public int PANStatus { get; set; }
        public int panRequestid { get; set; }

        public string DMTID { get; set; }
        public int DMTStatus { get; set; }

        public string BBPSID { get; set; }
        public int BBPSStatus { get; set; }
    }
}
