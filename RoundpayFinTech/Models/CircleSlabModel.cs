using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class CircleSlabModel
    {
        public bool IsAdmin { get; set; }
        public int SlabID { get; set; }
        public int APIID { get; set; }
        public IEnumerable<OperatorDetail> Ops { get; set; }
    }
}
