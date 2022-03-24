using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class HomeDisplay
    {
        public string AboutUs { get; set; }
        public string ContactUs { get; set; }
        public string Home { get; set; }
        public string Services { get; set; }
        public string Feature { get; set; }
        public string Testimonial { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string FullPage { get; set; }
        public int SectionID{ get; set; }
    }

    public class HomeDisplayRequest
    {
        public string Template { get; set; }
        public int ThemeID { get; set; }
        public int SectionID { get; set; }
        public int WID { get; set; }
    }
}
