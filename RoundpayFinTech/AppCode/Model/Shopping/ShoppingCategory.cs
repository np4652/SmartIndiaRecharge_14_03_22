using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ShoppingCategory
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public bool IsNextLevelExists { get; set; }
        public bool IsActive { get; set; }
        public decimal Commission { get; set; }
        public bool CommissionType { get; set; }
        public int ProductCount { get; set; }
        public string ImagePath { get; set; }
    }

    public class ShoppingSubCategoryLvl1 : ShoppingCategory
    {
        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }
    }

    public class ShoppingSubCategoryLvl2 : ShoppingSubCategoryLvl1
    {
        public int SubCategoryIDLvl2 { get; set; }
        public string SubCategoryNameLvl2 { get; set; }
        public string FilePath { get; set; }
    }

    public class ShoppingSubCategoryLvl3 : ShoppingSubCategoryLvl2
    {
        public int SubCategoryIDLvl3 { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string SubCategoryNameLvl3 { get; set; }
    }

    public class subCategoryModelLvl1
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<ShoppingSubCategoryLvl1> Level1 { get; set; }
    }

    public class subCategoryModelLvl2
    {
        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }
        public IEnumerable<ShoppingSubCategoryLvl2> subCategoryList { get; set; }
    }

    public class subCategoryModelLvl3
    {
        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }
        public IEnumerable<ShoppingSubCategoryLvl3> subCategoryList { get; set; }
    }

    public class IndexCategoryList
    {
        public List<ShoppingCategory> Categories { get; set; }
    }

    public class Vendors
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int vendorsID { get; set; }
        public string vendorsName { get; set; }
        public string IsActive { get; set; }
        public string Location { get; set; }
    }
}
