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
    public class ProcShoppingGetKeywordDetails : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcShoppingGetKeywordDetails(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new GetKeywordResponse();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@KeyWord", req.CommonStr)
            };
            int id = 1;
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.MainCategoryId = dt.Rows[0]["_CategoryID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CategoryID"]);
                    res.MainCategoryName = dt.Rows[0]["_CategoryName"] is DBNull ? "" : dt.Rows[0]["_CategoryName"].ToString();
                    res.CategoryId = dt.Rows[0]["_SubcategoryID1"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SubcategoryID1"]);
                    res.CategoryName = dt.Rows[0]["_CategoryName"] is DBNull ? "" : dt.Rows[0]["_CategoryName"].ToString();
                    res.SubCategoryId = dt.Rows[0]["_SubcategoryID2"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SubcategoryID2"]);
                    res.SubCategoryName = dt.Rows[0]["_SubCategoryName"] is DBNull ? "" : dt.Rows[0]["_SubCategoryName"].ToString();
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
        public string GetName() => "ProcShoppingGetKeywordDetails";
    }
}
