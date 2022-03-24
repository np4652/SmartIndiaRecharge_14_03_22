

using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class Theme
    {
        public int Id { get; set; }
        public string ThemeName { get; set; }
        public bool IsCurrentlyActive{ get; set; }
        public bool IsWLAllowed { get; set; }
        public bool IsOnlyForAdmin { get; set; }
    }

    public class SiteTemplate
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public string DemoPath { get; set; }
        public string DemoImage { get; set; }
        public IEnumerable<string> Assets { get; set; }
    }

    public class TemplatesAndThemes
    {
        public IEnumerable<Theme> Themes { get; set; }
        public IEnumerable<SiteTemplate> SiteTemplates { get; set; }
        public bool isOnlyForRP { get; set; }
    }

    public class IndexViewModel
    {
        public int WID { get; set; }
        public int SiteId { get; set; }
        public IEnumerable<string> Assets { get; set; }
        public SiteTemplateSection Content { get; set; }
    }
}
