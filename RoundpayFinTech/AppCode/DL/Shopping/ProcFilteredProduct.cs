using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFilteredProduct : IProcedure
    {
        private readonly IDAL _dal;

        public ProcFilteredProduct(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (ProductFilter)obj;

            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.UserId),
                new SqlParameter("@Lt", req.LoginTypeId),
                new SqlParameter("@CategoryID", req.CategoryID),
                new SqlParameter("@SubCategoryID1", req.SubCategoryID1),
                new SqlParameter("@SubCategoryID2", req.SubCategoryID2),
                new SqlParameter("@FilterIds", req.FilterIds),
                new SqlParameter("@OptionIds", req.OptionIds),
                new SqlParameter("@BrandIds", req.BrandIDs),
                new SqlParameter("@PriceRange", req.PriceRange)
            };
            var res = new List<ProductDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ProductDetail
                        {
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            CategoryID = dr["_CategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["_CategoryID"]),
                            SubCategoryID1 = dr["_SubCategoryID1"] is DBNull ? 0 : Convert.ToInt32(dr["_SubCategoryID1"]),
                            SubCategoryID2 = dr["_SubCategoryID2"] is DBNull ? 0 : Convert.ToInt32(dr["_SubCategoryID2"]),
                            ProductDetailID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            ProductID = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductCode = Convert.ToString(dr["_ProductCode"]),
                            VendorID = dr["_VendorID"] is DBNull ? 0 : Convert.ToInt32(dr["_VendorID"]),
                            BrandID = dr["_BrandID"] is DBNull ? 0 : Convert.ToInt32(dr["_BrandID"]),
                            //Quantity = dr["_Quantity"] is DBNull ? 0 : Convert.ToInt32(dr["_Quantity"]),
                            Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommissionType = dr["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_Commission"]),
                            DetailDescription = Convert.ToString(dr["_Description"]),
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            AdditionalTitle = dr["AdditionalTitle"] is DBNull ? "" : Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                        }
                        res.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_FilteredProduct";
    }
}
