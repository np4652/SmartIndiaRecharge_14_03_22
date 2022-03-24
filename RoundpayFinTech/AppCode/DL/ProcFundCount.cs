using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFundCount : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFundCount(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int LoginID = (int)obj;
            int count = 0;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID),
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    count = Convert.ToInt16(dt.Rows[0]["fundCount"]);
                }
            }
            catch (Exception ex)
            {
            }
            return count;
            
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "select count(1) fundCount from tbl_FundRequest with(nolock) where _BankId in(select _ID from tbl_Bank where _UserID=@LoginID) and _Status=1";
        }

    }
}
