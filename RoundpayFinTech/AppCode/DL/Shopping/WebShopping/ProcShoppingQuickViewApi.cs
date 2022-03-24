using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Fintech.AppCode.StaticModel;
using System.IO;
using System.Text;
using System.Linq;


namespace RoundpayFinTech.AppCode.DL.Shopping.WebShopping
{
    public class ProcShoppingQuickViewApi : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcShoppingQuickViewApi(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {

            var allProductInfos = new List<AllProductInfo>();
            var filterWithOptions = new List<FilterWithOption>();
            var addressMasterResponse = new AddressMasterResponse();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ProductDetailID", req.CommonInt),
                new SqlParameter("@LoginID", req.CommonStr)

            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(true);
                if (ds.Tables.Count > 0)
                {
                    DataTable dtfilterWithOptions = ds.Tables[0];//Table2 filterWithOptions
                    DataTable dtallProductInfos = ds.Tables[1]; //Table1 allProductInfos
                    DataTable dtShippingAddress = ds.Tables[2];//Table2 filterWithOptions
                    #region 3 Table3 ShippingAddress
                    if (dtShippingAddress.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtShippingAddress.Rows)
                        {
                            addressMasterResponse = new AddressMasterResponse
                            {
                                AddressId = dr["_id"] is DBNull ? 0 : Convert.ToInt32(dr["_id"]),
                                LoginId = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                                MobileNo = dr["_MobileNo"] is DBNull ? "" : dr["_MobileNo"].ToString(),
                                Name = dr["_CustomerName"] is DBNull ? "" : dr["_CustomerName"].ToString(),
                                AlternateMobileNo = dr["_MobileNo"] is DBNull ? "" : dr["_MobileNo"].ToString(),
                                Pincode = dr["_PIN"] is DBNull ? "" : dr["_PIN"].ToString(),
                                Area = dr["_Area"] is DBNull ? "" : dr["_Area"].ToString(),
                                Address = dr["_Address"] is DBNull ? "" : dr["_Address"].ToString(),
                                State = dr["StateName"] is DBNull ? "" : dr["StateName"].ToString(),
                                City = dr["City"] is DBNull ? "" : dr["City"].ToString(),
                                LandMark = dr["_LandMark"] is DBNull ? "" : dr["_LandMark"].ToString(),
                                EmailId = "",
                                AddressType = dr["_Title"] is DBNull ? "" : dr["_Title"].ToString(),
                                IsDefault = dr["_IsDefault"] is DBNull ? false : Convert.ToBoolean(dr["_IsDefault"])
                            };
                        }
                    }
                    #endregion
                    #region 1 Table1 allProductInfos
                    if (dtallProductInfos.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtallProductInfos.Rows)
                        {
                            var data = new AllProductInfo
                            {
                                IsCartAdded = dr["IsCart"] is DBNull ? 0 : Convert.ToInt32(dr["IsCart"]),
                                IsWishlistAdded = dr["IsWishList"] is DBNull ? 0 : Convert.ToInt32(dr["IsWishList"]),
                                SetName = dr["SetName"] is DBNull ? "" : dr["SetName"].ToString(),
                                Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                                ProductName = dr["productName"] is DBNull ? "" : dr["productName"].ToString(),
                                ProductCode = dr["productCode"] is DBNull ? "" : dr["productCode"].ToString(),
                                CategoryID = dr["CategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["CategoryID"]),
                                SubCategoryId = dr["subCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["subCategoryId"]),
                                SubCategoryName= dr["SubCategoryName"] is DBNull ? "" : dr["SubCategoryName"].ToString(),
                                CategoryName = dr["categoryName"] is DBNull ? "" : dr["categoryName"].ToString(),
                                MainCategoryID = dr["mainCategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["mainCategoryID"]),
                                MaincategoryName = dr["maincategoryName"] is DBNull ? "" : dr["maincategoryName"].ToString(),
                                ProductOptionSetId = dr["ProductOptionSetId"] is DBNull ? 0 : Convert.ToInt32(dr["ProductOptionSetId"]),
                                Discount = dr["Discount"] is DBNull ? 0 : Convert.ToInt32(dr["Discount"]),
                                MRP = dr["Mrp"] is DBNull ? 0 : Convert.ToInt32(dr["Mrp"]),
                                SellingPrice = dr["SellingPrice"] is DBNull ? 0 : Convert.ToInt32(dr["SellingPrice"]),
                                Description = dr["Description"] is DBNull ? "" : dr["Description"].ToString(),
                                Specification = dr["Specification"] is DBNull ? "" : dr["Specification"].ToString(),
                                RemainingQuantity = dr["RemainingQuantity"] is DBNull ? 0 : Convert.ToInt32(dr["RemainingQuantity"]),
                                POSId = dr["PosID"] is DBNull ? 0 : Convert.ToInt32(dr["PosID"]),
                                ProductDetailID = dr["ProductDetailId"] is DBNull ? 0 : Convert.ToInt32(dr["ProductDetailId"]),
                            };
                            data.AffiliateShareLink = req.CommonStr2 + "/GetProductDetails/" + data.POSId.ToString();
                            data.ShareLink = req.CommonStr2 + "/GetProductDetails/" + data.POSId.ToString();
                            string Domain = req.CommonStr2;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(DOCType.ProductImage);
                            sb.Append(data.ProductOptionSetId);
                            string path = sb.ToString();
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(req.CommonInt.ToString() + "_*_*");
                            FileInfo[] BigFiles = d.GetFiles(req.CommonInt.ToString() + "_*-1x*");
                            var FilImage=new { };
                            foreach(var item in BigFiles)
                            {
                                Files = Files.Where(e => e.Name != item.Name).ToArray();

                            }
                            if (Files.Length > 0)
                            {
                                data.FrontImage_100 = string.Concat(Domain, "/", path, "/", Files[0].Name ?? "");
                                data.BackImage_100 = string.Concat(Domain, "/", path, "/", Files[1].Name ?? "");
                                data.SideImage_100 = string.Concat(Domain, "/", path, "/", Files[2].Name ?? "");
                                data.SmallImage = string.Concat(Domain, "/", path, "/", Files[0].Name ?? "");

                            }
                            if (BigFiles.Length > 0)
                            {
                                data.FrontImage = string.Concat(Domain, "/", path, "/", BigFiles[0].Name ?? "");
                                data.BackImage = string.Concat(Domain, "/", path, "/", BigFiles[1].Name ?? "");
                                data.SideImage = string.Concat(Domain, "/", path, "/", BigFiles[2].Name ?? "");
                                data.FrontImage_1200 = string.Concat(Domain, "/", path, "/", BigFiles[0].Name ?? "");
                                data.BackImage_1200 = string.Concat(Domain, "/", path, "/", BigFiles[1].Name ?? "");
                                data.SideImage_1200 = string.Concat(Domain, "/", path, "/", BigFiles[2].Name ?? "");
                            }

                            allProductInfos.Add(data);
                        }
                    }
                    #endregion
                    #region 2 Table2 filterWithOptions
                    if (dtfilterWithOptions.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtfilterWithOptions.Rows)
                        {
                            filterWithOptions.Add(new FilterWithOption
                            {
                                FilterName = dr["FilterName"] is DBNull ? "" : dr["FilterName"].ToString(),
                                OptionName = dr["OptionName"] is DBNull ? "" : dr["OptionName"].ToString(),
                                POSID = dr["POSID"] is DBNull ? 0 : Convert.ToInt32(dr["POSID"])
                            });
                        }
                    }
                    #endregion                    
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return new GetQuickViewProduct
            {

                FilterWithOption = filterWithOptions ?? new List<FilterWithOption>(),
                AllProductInfo = allProductInfos ?? new List<AllProductInfo>(),
                DefaultAddress = addressMasterResponse
            };
        }

        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_ShoppingQuickViewApi";
    }
}
