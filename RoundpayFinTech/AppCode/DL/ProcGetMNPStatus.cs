using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMNPStatus : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetMNPStatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new MNPStsResp();
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@UserID", _req.LoginID)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    res.MNPStatus = dt.Rows[0]["_VerifyStatus"] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0]["_VerifyStatus"]);
                    res.MNPRemark = dt.Rows[0]["_Remark"] is DBNull ? string.Empty : dt.Rows[0]["_Remark"].ToString();
                    if (res.MNPStatus == MNPStatus.APPROVED)
                    {
                        res.OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]);
                        res.OpName = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                        res.UserName = dt.Rows[0]["_UserName"] is DBNull ? string.Empty : dt.Rows[0]["_UserName"].ToString();
                        res.Password = dt.Rows[0]["_Password"] is DBNull ? string.Empty : dt.Rows[0]["_Password"].ToString();
                        res.AppLink = dt.Rows[0]["_AppLink"] is DBNull ? string.Empty : dt.Rows[0]["_AppLink"].ToString();
                        res.MNPMobile = dt.Rows[0]["_MobileMNP"] is DBNull ? string.Empty : dt.Rows[0]["_MobileMNP"].ToString();
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetMNPStatus";


    }
}