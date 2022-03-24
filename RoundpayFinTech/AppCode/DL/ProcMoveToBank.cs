using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMoveToBank : IProcedure
    {
        private readonly IDAL _dal;

        public ProcMoveToBank(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            WalletRequest _req = (WalletRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginId", _req.LoginID),
                new SqlParameter("@IDs", _req.dt),
                new SqlParameter("@Remark", _req.Remark??string.Empty),
                new SqlParameter("@Status", _req.Status)
              };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();

                }
            }
            catch (Exception er)
            { }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_MoveToBank_Active";
        }
    }
}

