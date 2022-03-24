using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRailwaySave : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRailwaySave(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (IRSaveModel)obj;
            SqlParameter[] param =  {
                new SqlParameter("@reqEnc", req.EncodedRequest??string.Empty),
                new SqlParameter("@req", req.Request??string.Empty),
                new SqlParameter("@reservationId", req.reservationId??string.Empty),
                new SqlParameter("@txnAmount", req.txnAmount),
                new SqlParameter("@txnDate", req.txnDate??string.Empty),
                new SqlParameter("@RU", req.RU??string.Empty),
                new SqlParameter("@checkSum", req.checkSum??string.Empty),
                new SqlParameter("@ip", req.IP??string.Empty),
                new SqlParameter("@browser", req.Browser??string.Empty),
                new SqlParameter("@TranStatus", req.TranStatus),
                new SqlParameter("@response", req.Response??string.Empty),
                new SqlParameter("@encodedResponse", req.EncodedResponse??string.Empty),
                new SqlParameter("@isDoubleVerify", req.IsDoubleVerification),
                new SqlParameter("@IRSaveID", req.IRSaveID)
                };
            req.StatusCode = ErrorCodes.Minus1;
            req.Msg = ErrorCodes.TempError;
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    req.StatusCode = Convert.ToInt32(dt.Rows[0][0] is DBNull?ErrorCodes.Minus1:dt.Rows[0][0]);
                    req.Msg = dt.Rows[0]["Msg"] is DBNull?string.Empty: dt.Rows[0]["Msg"].ToString();
                    req.IRSaveID = Convert.ToInt32(dt.Rows[0]["IRSaveID"] is DBNull ? ErrorCodes.Minus1 : dt.Rows[0]["IRSaveID"]);
                    req.RU = dt.Rows[0]["RU"] is DBNull ? string.Empty : dt.Rows[0]["RU"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 0
                });
            }
            return req;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_IRSave";
    }
    public class ProcRailwayGetRU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRailwayGetRU(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (IRSaveModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@irSaveId", req.IRSaveID)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) == 1)
                {
                    req.StatusCode = Convert.ToInt32(dt.Rows[0][0]);
                    req.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    req.RU = Convert.ToString(dt.Rows[0]["RU"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserId
                });
            }
            return req;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_IRGetRU";
    }

    public class ProcLogRailwayReqResp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcLogRailwayReqResp(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (LogRailwayReqRespModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@methodName", req.MethodName),
                new SqlParameter("@request", req.Request),
                new SqlParameter("@response", req.Response),
                new SqlParameter("@browser", req.Browser),
                new SqlParameter("@ip", req.IP)
            };
            try
            {
                _dal.ExecuteProcedure(GetName(), param);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 0
                });
            }
            return null;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_IRLogReqResp";
    }
}
