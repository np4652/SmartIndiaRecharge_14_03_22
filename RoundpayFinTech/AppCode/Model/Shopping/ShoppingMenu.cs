using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NewShoping = RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ShoppingMenu: NewShoping.Menu
    {
        public int MenuType { get; set; }
        public IEnumerable<MenuLevel1> MenuLevel1 { get; set; }
    }

    public class MenuLevel1
    {
        public int CategoryID { get; set; }
        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }
        public string FilePath { get; set; }
        public IEnumerable<NewShoping.SubCategory> MenuLevel2 { get; set; }
    }
}
