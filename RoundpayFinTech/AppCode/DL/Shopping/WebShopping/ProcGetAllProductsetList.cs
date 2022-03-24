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

namespace RoundpayFinTech.AppCode.DL.Shopping.WebShopping
{
    public class ProcGetAllProductsetList : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetAllProductsetList(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<ProductOptionSetInfoList>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WebsiteID", req.CommonStr5),
                new SqlParameter("@FilterType", req.CommonStr2),
                new SqlParameter("@FilterTypeID", req.CommonInt3),
                new SqlParameter("@Start", req.CommonInt),
                new SqlParameter("@PageLimit", req.CommonInt2),
                new SqlParameter("@LoginId", 1),
                new SqlParameter("@OrderBy", req.CommonStr3),
                new SqlParameter("@OrderByType", req.CommonStr4),
                new SqlParameter("@FilterOptionTypeIDS", req.CommonStr),
                new SqlParameter("@BaseUrl", ""),
                new SqlParameter("@KeywordId", req.CommonInt4),
            };
            int id = 1;
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ProductOptionSetInfoList
                        {

                            POSId = dr["POSId"] is DBNull ? 0 : Convert.ToInt32(dr["POSId"]),
                            Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                            SetName = "",
                            ProductName = dr["ProductName"] is DBNull ? "" : dr["ProductName"].ToString(),
                            SubCatName = dr["SubCategoryName"] is DBNull ? "" : dr["SubCategoryName"].ToString(),
                            SubCategoryId = dr["SubCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubCategoryId"]),
                            CategoryName = dr["CategoryName"] is DBNull ? "" : dr["CategoryName"].ToString(),
                            MainCategoryName = dr["MainCategoryName"] is DBNull ? "" : dr["MainCategoryName"].ToString(),
                            MainCategoryID = dr["MainCategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["MainCategoryId"]),
                            CategoryId = dr["CategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["CategoryId"]),
                            RemainingQuantity = 0,
                            SellingPrice = dr["SellingPrice"] is DBNull ? 0 : Convert.ToInt32(dr["SellingPrice"]),
                            MRP = dr["MRP"] is DBNull ? 0 : Convert.ToInt32(dr["MRP"]),
                            Discount = dr["Discount"] is DBNull ? 0 : Convert.ToInt32(dr["Discount"]),
                            IsCartAdded = 0,
                            FrontImage = dr["FrontImage"] is DBNull ? "" : dr["FrontImage"].ToString(),
                            SmallImage = dr["SmallImage"] is DBNull ? "" : dr["SmallImage"].ToString(),
                            Description = dr["_Description"] is DBNull ? "" : dr["_Description"].ToString(),
                            TotalRecords = dr["TOTALRECORDS"] is DBNull ? 0 : Convert.ToInt32(dr["TOTALRECORDS"])
                        };
                        data.AffiliateShareLink = req.CommonStr1 + "/GetProductDetails/" + data.POSId.ToString();
                        data.ShareLink = req.CommonStr1 + "/GetProductDetails/" + data.POSId.ToString();

                        string Domain = req.CommonStr1;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DOCType.ProductImage);
                        sb.Append(data.FrontImage);
                        string path = sb.ToString();
                        DirectoryInfo d = new DirectoryInfo(path);
                        FileInfo[] Files = d.GetFiles(data.POSId.ToString() + "_*");
                        FileInfo[] BigFiles = d.GetFiles(data.POSId.ToString() + "_*-1x*");
                        if (Files.Length > 0)
                        {
                            data.SmallImage = string.Concat(Domain, "/", path, "/", Files[0].Name);

                        }
                        if (BigFiles.Length > 0)
                        {
                            data.FrontImage = string.Concat(Domain, "/", path, "/", BigFiles[0].Name);

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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetAllProductsetList";
    }
}
