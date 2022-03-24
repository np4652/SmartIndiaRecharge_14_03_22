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
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcProDescription : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcProDescription(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var res = new ProductDetailForUser();
            var option = new List<FilterOption>();
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.UserID),
                new SqlParameter("@ProductDetailID",req.CommonInt),
                new SqlParameter("@BrowserID",req.CommonStr),
            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param);
                var dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        option.Add(new FilterOption
                        {
                            FilterID=Convert.ToInt32(dr["_FilterID"]),
                            FilterOptionID=Convert.ToInt32(dr["_FilterOptionID"]),
                        });
                    }
                }
                res.selectedOption = option;
                if (ds.Tables.Count > 0)
                    dt = ds.Tables[1];
                if (dt.Rows.Count > 0)
                {
                    res.Quantity = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0][0]);
                    res.ShippingCharges = dt.Rows[0][1] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0][1]);
                }

                if (ds.Tables.Count > 1)
                    dt = ds.Tables[2];
                if (dt.Rows.Count > 0)
                {
                    res.ProductID = Convert.ToInt32(dt.Rows[0]["_ProductID"]);
                    res.ProductName = Convert.ToString(dt.Rows[0]["_ProductName"]);
                    res.Specification = Convert.ToString(dt.Rows[0]["_Specification"]);
                    res.Discription = Convert.ToString(dt.Rows[0]["_Description"]);
                    res.CommonDiscription = Convert.ToString(dt.Rows[0]["_CommonDiscription"]);
                    res.MRP = Convert.ToDecimal(dt.Rows[0]["_MRP"]);
                    res.Discount = Convert.ToDecimal(dt.Rows[0]["_discount"]);
                    res.DiscountType = Convert.ToBoolean(dt.Rows[0]["_DiscountType"]);
                    res.SellingPrice = Convert.ToDecimal(dt.Rows[0]["_SellingPrice"]);
                    res.AdditionalTitle = Convert.ToString(dt.Rows[0]["AdditionalTitle"]);
                    string path = DOCType.ProductImagePath.Replace("{0}", res.ProductID.ToString());
                    res.ImgList = new List<string>();
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo d = new DirectoryInfo(path);
                        FileInfo[] Files = d.GetFiles(req.CommonInt2.ToString() + "_*");
                        
                        foreach (FileInfo file in Files)
                        {
                            res.ImgList.Add(file.Name);
                        }

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
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_ProDescription";
    }
}