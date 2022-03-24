using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcgetRequestApproval : IProcedure
    {
        private readonly IDAL _dal;
        public ProcgetRequestApproval(IDAL dal) => _dal = dal;
        public string GetName() => "select distinct ISNUll(_ApprovalID,0) _ApprovalID , dbo.CustomFormat(_ApproveDate) _ApproveDate from tbl_WalletRequest where cast(_ApproveDate as date) = Cast(@ApproveDate as Date)";
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ApproveDate", _req.CommonStr?? DateTime.Now.ToString("dd MMM yyyy"))
            };
            var _alist = new List<ResponseStatus>();
            try
            {
                var dt = _dal.Get(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ResponseStatus
                    {
                        Statuscode = ErrorCodes.One,
                        Msg = "Success",
                        CommonInt = dr["_ApprovalID"] is DBNull ? 0 : Convert.ToInt32(dr["_ApprovalID"]),
                        CommonStr2 = Convert.ToString(dr["_ApproveDate"]),
                    };
                    _alist.Add(item);
                }
            }
            catch (Exception er)
            {

            }
            return _alist;
        }

        public object Call() => throw new NotImplementedException();
    }
}
