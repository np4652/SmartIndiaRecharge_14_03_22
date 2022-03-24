using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcTrackBBPSComplain : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTrackBBPSComplain(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@TableID",req.CommonInt),
                new SqlParameter("@LiveID",req.CommonStr??string.Empty),
                new SqlParameter("@complaintType",req.CommonInt2)
            };
            var res = new GenerateBBPSComplainProcResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        res.TableID = dt.Rows[0]["_TableID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TableID"]);
                        res.ComplainStatus = dt.Rows[0]["_ComplainStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ComplainStatus"]);
                        res.ReferenceID = dt.Rows[0]["_ReferenceID"] is DBNull ? string.Empty : dt.Rows[0]["_ReferenceID"].ToString();
                    }
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
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_TrackBBPSComplain";
    }
}
