using Microsoft.AspNetCore.Mvc.Rendering;

namespace RoundpayFinTech.Models
{
    public class SelectAPIList
    {
        public int APIID { get; set; }
        public SelectList selectList { get; set; }
    }
}
