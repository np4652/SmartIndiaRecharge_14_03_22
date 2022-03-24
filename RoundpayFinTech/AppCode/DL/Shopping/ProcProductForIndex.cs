using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
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
    public class ProcProductForIndex : IProcedure
    {
        private readonly IDAL _dal;

        public ProcProductForIndex(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (ProductFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.UserId),
                new SqlParameter("@Lt", req.LoginTypeId),
                new SqlParameter("@CategoryID", req.CategoryID),
                new SqlParameter("@SubCategoryID1", req.SubCategoryID1),
                new SqlParameter("@SubCategoryID2", req.SubCategoryID2),
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
                            CategoryName = Convert.ToString(dr["CategoryName"]),
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
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommissionType = dr["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_Commission"]),
                            DetailDescription = Convert.ToString(dr["_Description"]),
                            AdditionalTitle = Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        data.ImgUrlList = new List<string>();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrlList.Add(file.Name);
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
        public string GetName() => "proc_ProductForIndex";
    }

    public class ProcProductTrending : IProcedure
    {
        private readonly IDAL _dal;

        public ProcProductTrending(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (ProductFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.UserId),
                new SqlParameter("@Lt", req.LoginTypeId),
                new SqlParameter("@CategoryID", req.CategoryID),
                new SqlParameter("@SubCategoryID1", req.SubCategoryID1),
                new SqlParameter("@SubCategoryID2", req.SubCategoryID2),
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
                            CategoryName = Convert.ToString(dr["CategoryName"]),
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
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommissionType = dr["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_Commission"]),
                            DetailDescription = Convert.ToString(dr["_Description"]),
                            B2BSellingPrice = dr["_B2BSellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_B2BSellingPrice"]),
                            B2BDiscount = dr["_B2BDiscount"] is DBNull ? 0 : Convert.ToDecimal(dr["_B2BDiscount"]),
                            B2BDiscountType = dr["_B2BDiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_B2BDiscountType"]),
                            B2BCommission = dr["_B2BCommission"] is DBNull ? 0 : Convert.ToDecimal(dr["_B2BCommission"]),
                            B2BCommissionType = dr["_B2BCommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_B2BCommission"]),
                            AdditionalTitle = Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        data.ImgUrlList = new List<string>();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrlList.Add(file.Name);
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
        public string GetName() => "proc_ProductTrending";
    }

    public class ProcProductNewArrival : IProcedure
    {
        private readonly IDAL _dal;

        public ProcProductNewArrival(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (ProductFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.UserId),
                new SqlParameter("@Lt", req.LoginTypeId),
                new SqlParameter("@CategoryID", req.CategoryID),
                new SqlParameter("@SubCategoryID1", req.SubCategoryID1),
                new SqlParameter("@SubCategoryID2", req.SubCategoryID2),
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
                            CategoryName = Convert.ToString(dr["CategoryName"]),
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
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommissionType = dr["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_Commission"]),
                            DetailDescription = Convert.ToString(dr["_Description"]),
                            AdditionalTitle = Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        data.ImgUrlList = new List<string>();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrlList.Add(file.Name);
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
        public string GetName() => "proc_ProductNewArrival";
    }

    public class ProcProductSimilar : IProcedure
    {
        private readonly IDAL _dal;

        public ProcProductSimilar(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (ProductFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.UserId),
                new SqlParameter("@Lt", req.LoginTypeId),
                new SqlParameter("@ProductId", req.ProductId),
                new SqlParameter("@PDId", req.ProductDetailId)
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
                            CategoryName = Convert.ToString(dr["CategoryName"]),
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
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommissionType = dr["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_Commission"]),
                            DetailDescription = Convert.ToString(dr["_Description"]),
                            AdditionalTitle = Convert.ToString(dr["AdditionalTitle"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        data.ImgUrlList = new List<string>();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrlList.Add(file.Name);
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
        public string GetName() => "proc_ProductSimilar";
    }
}
