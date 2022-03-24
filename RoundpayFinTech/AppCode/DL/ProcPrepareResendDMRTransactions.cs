using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPrepareResendDMRTransactions : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcPrepareResendDMRTransactions(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TIDs", _req.str??""),
                new SqlParameter("@IP", _req.CommonStr?? ""),
                new SqlParameter("@Browser", _req.CommonStr2 ?? "")
            };
            
            List<PrepairedTransactionReq> _lst = new List<PrepairedTransactionReq>();

            PrepairedTransactionReq _res = new PrepairedTransactionReq
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            _res = new PrepairedTransactionReq
                            {
                                TID = Convert.ToInt32(row["TID"]),
                                TransactionID = row["TransactionID"].ToString(),
                                UserID = Convert.ToInt32(row["UserID"]),
                                AccountNo = row["Account"].ToString(),
                                RequestedAmount = Convert.ToDecimal(row["RequestedAmount"]),
                                OID = Convert.ToInt32(row["OID"]),
                                BeneID = row["BeneID"] is DBNull ? "" : row["BeneID"].ToString(),
                                TransactionMode = row["TransactionMode"] is DBNull ? "" : row["TransactionMode"].ToString(),
                                APIID = Convert.ToInt32(row["APIID"]),
                                API = row["API"].ToString(),
                                APICode = row["APICode"].ToString(),
                                SenderNo = row["SenderNo"].ToString(),
                                PAN = row["PAN"].ToString(),
                                OutletLatLong = row["OutletLatLong"].ToString(),
                                APIOutletID = row["APIOutletID"].ToString(),
                                OutletPincode = row["OutletPincode"].ToString()
                            };
                            _lst.Add(_res);
                        }
                    }
                    else
                    {
                        _lst.Add(_res);
                    }
                }
                else
                {
                    _lst.Add(_res);
                }

            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _lst;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_PrepareResendDMRTransactions";
        }
    }
}