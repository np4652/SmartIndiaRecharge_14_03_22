using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUsersDataForAEPS : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUsersDataForAEPS(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
            new SqlParameter("@UserID",req.LoginID),
            new SqlParameter("@OutletID",req.CommonInt),
            new SqlParameter("@APICode",req.CommonStr??string.Empty)
            };

            var res = new UserDataForAEPS
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? ErrorCodes.Minus1 : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if (res.Statuscode == ErrorCodes.One) {
                        res.UserID = req.LoginID;
                        res.OutletID = req.CommonInt;
                        res.APIID = dt.Rows[0]["_APIID"] is DBNull ? ErrorCodes.Minus1 : Convert.ToInt32(dt.Rows[0]["_APIID"]);
                        res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                        res.LatLong = dt.Rows[0]["_LatLong"] is DBNull ? string.Empty : dt.Rows[0]["_LatLong"].ToString();
                        res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? string.Empty : dt.Rows[0]["_Pincode"].ToString();
                        res.APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                        res.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        res.APICode = req.CommonStr??string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetUsersDataForAEPS";
    }
}
