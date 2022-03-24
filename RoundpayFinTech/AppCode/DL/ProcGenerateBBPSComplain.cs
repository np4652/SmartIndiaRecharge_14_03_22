using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGenerateBBPSComplain : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGenerateBBPSComplain(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GenerateBBPSComplainProcReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@StoreOutletID",req.StoreOutletID),
                new SqlParameter("@OutletID",req.OutletID),
                new SqlParameter("@ComplainType",req.ComplainType),
                new SqlParameter("@TransactionDoneAt",req.TransactionDoneAt),
                new SqlParameter("@TransactionID",req.TransactionID),
                new SqlParameter("@ParticipationType",req.ParticipationType),
                new SqlParameter("@Reason",req.Reason),
                new SqlParameter("@Description",req.Description),
                new SqlParameter("@MobileNo",req.MobileNo),
                new SqlParameter("@OID",req.OID)
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
                    res.ErrorCode = dt.Rows[0]["_ErrorCode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ErrorCode"]);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                        res.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        res.VendorID = dt.Rows[0]["_VendorID"] is DBNull ? string.Empty : dt.Rows[0]["_VendorID"].ToString();
                        res.BillerID = dt.Rows[0]["_BillerID"] is DBNull ? string.Empty : dt.Rows[0]["_BillerID"].ToString();
                        res.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        res.TableID = dt.Rows[0]["_TableID"] is DBNull ?0 : Convert.ToInt32(dt.Rows[0]["_TableID"]);
                        res.WID = dt.Rows[0]["_WID"] is DBNull ?0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GenerateBBPSComplain";
    }
}
