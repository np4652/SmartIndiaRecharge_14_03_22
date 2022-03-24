using System.Collections.Generic;
using NewShoping = RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System.Data;
using Fintech.AppCode.StaticModel;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ProductModel
    {
        public IEnumerable<NewShoping.Menu> MainCategory { get; set; }
        public IEnumerable<NewShoping.CategoryList> CategoryList { get; set; }
        public IEnumerable<NewShoping.SubCategory> SubCategory { get; set; }
        public IEnumerable<MasterProduct> MasterProduct { get; set; }
        public IEnumerable<Brand> Brands { get; set; }
        public IEnumerable<Vendors> Vendors { get; set; }
        public AddProductModal Detail { get; set; }
        public MasterProduct MasterProductDetail { get; set; }
        public int Role { get; set; }
    }

    public class MasterProduct
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryID1 { get; set; }
        public int SubCategoryID2 { get; set; }
        public string Description { get; set; }
        public string ProductName { get; set; }
        public decimal WalletDeductionPerc { get; set; }        
        public bool AdminCommType { get; set; }        
        public decimal MinAdminComm { get; set; }
        public string Keyword { get; set; }

    }

    public class OptionSet
    {
        public List<string> Filtres { get; set; }
        public List<string> Option { get; set; }

    }
    public class ProductDetail : MasterProduct
    {
        public int ProductDetailID { get; set; }
        public string ProductCode { get; set; }
        public string Batch { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal VendorPrice { get; set; }
        public decimal AdminProfit { get; set; }
        public decimal Discount { get; set; }
        public bool DiscountType { get; set; }
        public decimal Commission { get; set; }
        public decimal ShippingCharges { get; set; }
        public bool CommissionType { get; set; }
        public int Quantity { get; set; }
        public int ShippingMode { get; set; }
        public List<string> FilterWithOption { get; set; }
        public DataTable FilterDetail { get; set; }
        public OptionSet optionSet { get; set; }
        public string DetailDescription { get; set; }
        public string Specification { get; set; }
        public string ImgUrl { get; set; }
        public List<string> ImgUrlList { get; set; }
        public decimal DiscountCount { get; set; }
        public decimal shippingDiscount { get; set; }
        public decimal WeightInKG { get; set; }
        public int ReturnApplicable { get; set; }
        public bool IsTrending { get; set; }
        public bool IsDeleted { get; set; }
        public decimal B2BSellingPrice { get; set; }
        public decimal B2BVendorPrice { get; set; }
        public decimal B2BAdminProfit { get; set; }
        public decimal B2BDiscount { get; set; }
        public bool B2BDiscountType { get; set; }
        public decimal B2BCommission { get; set; }
        public decimal B2BShippingCharges { get; set; }
        public bool B2BCommissionType { get; set; }
        public int B2BShippingMode { get; set; }
        public decimal B2BDiscountCount { get; set; }
        public decimal B2BshippingDiscount { get; set; }
        public string AdditionalTitle { get; set; }
        public string Images { get; set; }
    }

    public class ProductWithMaster
    {
        public IEnumerable<MasterProduct> MasterProduct { get; set; }
        public IEnumerable<ProductDetail> ProductDetail { get; set; }
    }

    public class AddProductModal
    {
        public ProductDetail ProductDetail { get; set; }
        public IEnumerable<ProductFilterDetail> FilterDetail { get; set; }
    }

    public class CategoriesForIndex
    {
        public int MainCatId { get; set; }
        public string MainCatName { get; set; }
        public List<ShoppingSubCategoryLvl2> SubCategories { get; set; }
        public List<string> OfferImgPath { get; set; }
    }

    public class ProductForIndex
    {
        public int MainCatId { get; set; }
        public string MainCatName { get; set; }
        public List<ShoppingSubCategoryLvl2> SubCategories { get; set; }
        public List<ProductDetail> ProductDetail { get; set; }
    }
}
