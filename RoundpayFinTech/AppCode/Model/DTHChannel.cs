using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class DTHChannel
    {
        public int ID { get; set; }
        public string ChannelName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<ChannelCategory> categories { get; set; }
        public bool Del { get; set; }
        public bool IsActive { get; set; }
    }
    public class DTHChannelReq:DTHChannel
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
    }
}
