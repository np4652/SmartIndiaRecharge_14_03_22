using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetFilterOptionByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetFilterOptionByID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new FilterOption();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@ID", req.CommonInt)
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.ID = Convert.ToInt32(dt.Rows[0]["_ID"], CultureInfo.InvariantCulture);
                    res.FilterID = Convert.ToInt32(dt.Rows[0]["_FilterID"], CultureInfo.InvariantCulture);
                    res.Option = Convert.ToString(dt.Rows[0]["_Option"], CultureInfo.InvariantCulture);
                    res.OptionalID = Convert.ToString(dt.Rows[0]["_OptionalDetail"], CultureInfo.InvariantCulture);
                    res.IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"], CultureInfo.InvariantCulture);
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

        public object Call() => throw new NotImplementedException();
        public string GetName() => @"select * from Master_FilterOptions where _ID=@ID";
    }
}
