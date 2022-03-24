using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class ChannelUnderCategory:DTHChannelCategory
    {
        public IEnumerable<DTHChannel> channels { get; set; }
    }
}
