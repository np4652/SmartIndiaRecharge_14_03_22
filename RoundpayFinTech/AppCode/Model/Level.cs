using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class Level
    {
        public int UserID { get; set; }
        public int LevelCount { get; set; }
        public int LevelNo { get; set; }
        public string Parent { get; set; }
        public int ParentId { get; set; }
        public int TotalCountAT { get; set; }
        
    }
}
