
using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class DTHChannelMap:CommonReq
    {
        public int PackageID { get; set; }
        public int ChannelID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
        public List<ChannelCategory> categories { get; set; }
        public List<DTHChannel> Channels { get; set; }
    }
}
