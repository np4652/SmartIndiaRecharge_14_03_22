using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel
{
    public class SubCategory
    {
        public int Id { get; set; }
        public int SubCategoryId { get; set; }
        public int ParentId { get; set; }
        public int ProductCount { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string subCategoryImage { get; set; }
        public decimal Commission { get; set; }
        public bool CommissionType { get; set; }
    }

    public class SubCategoryMenus
    {
        public int SubCategoryId { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string SubCategoryImage { get; set; }
    }
    public class CategoryList
    {
        public int Id { get; set; }
        public IEnumerable<SubCategory> Subcategory { get; set; }
        public int ParentId { get; set; }
        public int SubCategoryId { get; set; }
        public int ProductCount { get; set; }
        public int mainCategoryID { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; }
        public string image { get; set; }
        public string icon { get; set; }
        public string IconeType { get; set; }
        public decimal Commission { get; set; }
        public bool CommissionType { get; set; }
    }
    public class Menu
    {

        public int Id { get; set; }
        public int MainCategoryID { get; set; }
        public int ProductCount { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string CategoryString { get; set; }
        public string MainCategoryImage { get; set; }
        public string icon { get; set; }
        public string IconeType { get; set; }
        public decimal Commission { get; set; }
        public bool CommissionType { get; set; }
        public IEnumerable<CategoryList> CategoryList { get; set; }

    }
    public class CategoryModelLvl1
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<CategoryList> Level1 { get; set; }
    }
    public class ShoppingCategoryLvl1 : CategoryList
    {
        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }
    }
    public class SubCategoryModelLvl2New
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<SubCategory> subCategoryList { get; set; }
    }
}
