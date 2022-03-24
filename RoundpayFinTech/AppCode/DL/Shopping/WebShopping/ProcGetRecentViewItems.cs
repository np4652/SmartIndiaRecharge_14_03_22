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
using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.AppCode.DL.Shopping.WebShopping
{
    public class ProcGetRecentViewItems : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetRecentViewItems(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<RecentViewModel>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WebsiteID", req.CommonStr),
                new SqlParameter("@BrowserId", req.CommonStr2),
                  new SqlParameter("@LoginID", req.CommonStr2),
            };
            int id = 1;
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new RecentViewModel
                        {
                            ProductId = dr["ProductId"] is DBNull ? 0 : Convert.ToInt32(dr["ProductId"]),
                            POSId = dr["POSId"] is DBNull ? 0 : Convert.ToInt32(dr["POSId"]),
                            Title = dr["Title"] is DBNull ? "" : dr["Title"].ToString(),
                            ProductName = dr["ProductName"] is DBNull ? "" : dr["ProductName"].ToString(),
                            RemainingQuantity = 0,
                            SellingPrice = dr["SellingPrice"] is DBNull ? 0 : Convert.ToInt32(dr["SellingPrice"]),
                            MRP = dr["MRP"] is DBNull ? 0 : Convert.ToInt32(dr["MRP"]),
                            Discount = dr["Discount"] is DBNull ? 0 : Convert.ToInt32(dr["Discount"]),
                            IsCartAdded = 0,
                            BrowserId = req.CommonStr2,
                            WebsiteId = req.CommonStr,
                            SubcategoryName = dr["SubcategoryName"] is DBNull ? "" : dr["SubcategoryName"].ToString(),
                            SubcategoryId = dr["SubcategoryId"] is DBNull ? 0 : Convert.ToInt32(dr["SubcategoryId"]),
                        };
                        data.AffiliateShareLink = req.CommonStr1 + "/GetProductDetails/" + data.POSId.ToString();
                        data.ShareLink = req.CommonStr1 + "/GetProductDetails/" + data.POSId.ToString();
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
        public string GetName() => "Proc_GetRecentViewItems";
    }

    public class ProcSaveRecentView : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcSaveRecentView(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            ResponseStatus res = new ResponseStatus();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.CommonInt),
                  new SqlParameter("@BrowserID", req.CommonStr),
                   new SqlParameter("@ProductDetailID", req.CommonInt2)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
        public string GetName() => "proc_SaveRecentView";
    }
}
