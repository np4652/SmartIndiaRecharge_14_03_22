namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    
    public class Video
    {
        public int ID { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }


    }
    public class videoDetailReq : Video
    {
        public int LoginID { get; set; }
        public int LT { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }

    }
}
