using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.Models
{
    public class ErrorAPICodeModel
    {
        public IEnumerable<ErrorCodeDetail> ErrorCodeDetails { get; set; }
        public IEnumerable<APIGroupDetail> APIGroupDetails { get; set; }
        public List<APIErrorCode> APIErrorCodes { get; set; }
    }
}
