using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAdditionalService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAdditionalService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID", req.UserID),
                new SqlParameter("@OutletID", req.CommonInt)
            };
            var resp = new GetAddService { 
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var data = new List<AddonServ>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            data.Add(new AddonServ
                            {
                                UID = row["_UID"] is DBNull ? 0 : Convert.ToInt32(row["_UID"]),
                                OpTypeID = row["OpTypeID"] is DBNull ? 0 : Convert.ToInt32(row["OpTypeID"]),
                                OID = row["OID"] is DBNull ? 0 : Convert.ToInt32(row["OID"]),
                                DisplayName = row["DisplayName"] is DBNull ? string.Empty : row["DisplayName"].ToString(),
                                IsActive = row["IsActive"] is DBNull ? false : Convert.ToBoolean(row["IsActive"]),
                                ServiceChargeDeuctionType = row["ServiceChargeDeuctionType"] is DBNull ? 0 : Convert.ToInt32(row["ServiceChargeDeuctionType"]),
                                IsIDLimitByOpertor = row["IsIDLimitByOpertor"] is DBNull ? false : Convert.ToBoolean(row["IsIDLimitByOpertor"]),
                                IDLimit = row["_IDLimit"] is DBNull ? 0 : Convert.ToInt32(row["_IDLimit"])
                            });
                        }
                    }
                    
                    resp.AddonServList = data;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = GetName(),
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }

            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAdditionalService";
    }
}
