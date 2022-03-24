using RoundpayFinTech.AppCode.Model.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping.WebShopping
{
    public class Menu
    {
    }
    public class maincategoryMenus
    {
        public int MainCategoryID { get; set; }
        public string Name { get; set; }
        public int? Commission { get; set; }
        public bool CommissionType { get; set; }
        public string Icone { get; set; }
        public string BannerImage { get; set; }
        public string IconeType { get; set; }



    }

    public class subCategoryMenus
    {
        public int SubCategoryId { get; set; }
        public int CategoryID { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public int? Commission { get; set; }
        public bool CommissionType { get; set; }
        public string Icone { get; set; }
        public string subCategoryImage { get; set; }
        public string IconeType { get; set; }
    }


    public class MenuResult : AppResponse
    {
        public IEnumerable<maincategoryMenus> maincategoryMenus { get; set; }
        public IEnumerable<subCategoryMenus> subCategoryMenus { get; set; }
    }



}
