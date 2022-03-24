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
    public class ProcGetAllWishListItems : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetAllWishListItems(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<WishListResponse>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserId", req.CommonInt2)
            };
            int id = 1;
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new WishListResponse
                        {
                            ProductId = dr["ProductId"] is DBNull ? 0 : Convert.ToInt32(dr["ProductId"]),
                            POSId = dr["POSId"] is DBNull ? 0 : Convert.ToInt32(dr["POSId"]),
                            Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                            SetName = dr["SetName"] is DBNull ? "" : dr["SetName"].ToString(),
                            RemainingStock = dr["RemainingStock"] is DBNull ? 0 : Convert.ToInt32(dr["RemainingStock"]),
                            SellingPrice = dr["SellingPrice"] is DBNull ? 0 : Convert.ToInt32(dr["SellingPrice"]),
                            MRP = dr["MRP"] is DBNull ? 0 : Convert.ToInt32(dr["MRP"]),
                            Discount = dr["Discount"] is DBNull ? 0 : Convert.ToInt32(dr["Discount"]),
                            AffiliateShareLink = req.CommonStr1,
                            ShareLink = req.CommonStr1,
                            IsCartAdded = dr["IsCartAdded"] is DBNull ? 0 : Convert.ToInt32(dr["IsCartAdded"]),
                            WishListID = dr["WishListID"] is DBNull ? 0 : Convert.ToInt32(dr["WishListID"]),
                        };
                        string Domain = req.CommonStr1;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DOCType.ProductImage);
                        sb.Append(data.ProductId);
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
        public string GetName() => "Proc_GetAllWishListItems";
    }
}
