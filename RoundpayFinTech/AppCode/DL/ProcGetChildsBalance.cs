using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetChildsBalance : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetChildsBalance(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ReffID",req.CommonInt)
            };
            var _res = new UserBalnace();
            try
            {

                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0) {
                    _res.Balance = Convert.ToDecimal(dt.Rows[0]["_Balance"] is DBNull ? 0 : dt.Rows[0]["_Balance"]);
                    _res.UBalance = Convert.ToDecimal(dt.Rows[0]["_UBalance"] is DBNull ? 0 : dt.Rows[0]["_UBalance"]);
                    _res.BBalance = Convert.ToDecimal(dt.Rows[0]["_BBalance"] is DBNull ? 0 : dt.Rows[0]["_BBalance"]);
                    _res.CBalance = Convert.ToDecimal(dt.Rows[0]["_CBalance"] is DBNull ? 0 : dt.Rows[0]["_CBalance"]);
                    _res.IDBalnace = Convert.ToDecimal(dt.Rows[0]["_IDBalnace"] is DBNull ? 0 : dt.Rows[0]["_IDBalnace"]);
                    _res.PacakgeBalance = Convert.ToDecimal(dt.Rows[0]["_PackageBalance"] is DBNull ? 0 : dt.Rows[0]["_PackageBalance"]);
                   
                    _res.IsBalance = Convert.ToBoolean(dt.Rows[0]["_IsPrepaid"] is DBNull ? false : dt.Rows[0]["_IsPrepaid"]);
                    _res.IsUBalance = Convert.ToBoolean(dt.Rows[0]["_IsUtility"] is DBNull ? false : dt.Rows[0]["_IsUtility"]);
                    _res.IsBBalance = Convert.ToBoolean(dt.Rows[0]["_IsBank"] is DBNull ? false : dt.Rows[0]["_IsBank"]);
                    _res.IsCBalance = Convert.ToBoolean(dt.Rows[0]["_IsCard"] is DBNull ? false : dt.Rows[0]["_IsCard"]);
                    _res.IsIDBalance = Convert.ToBoolean(dt.Rows[0]["_IsRegID"] is DBNull ? false : dt.Rows[0]["_IsRegID"]);
                    _res.IsPacakgeBalance = Convert.ToBoolean(dt.Rows[0]["_IsPackage"] is DBNull ? false : dt.Rows[0]["_IsPackage"]);
                }
            }
            catch (Exception)
            {
            }
            return _res;
        }

        //public Task<object> Call()
        //{
        //    throw new NotImplementedException();
        //}
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetChildsBalance";
       
    }
}
