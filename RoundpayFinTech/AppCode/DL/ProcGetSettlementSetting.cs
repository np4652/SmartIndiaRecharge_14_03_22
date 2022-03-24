using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSettlementSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSettlementSetting(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.CommonInt)
            };
            List<SettlementType> settlementType = new List<SettlementType>();
            UserwiseSettSetting userwiseSettSetting = new UserwiseSettSetting();
            SettlementSetting resp = new SettlementSetting
            {
                StatusCode = -1,
                Msg = ErrorCodes.TempError,
                settlementType = settlementType,
                userwiseSettSetting = userwiseSettSetting
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtMST = new DataTable();
                DataTable dtUSetting = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    resp.StatusCode = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                    resp.Msg = ds.Tables[0].Rows[0]["Msg"].ToString();
                    dtMST = ds.Tables[0];
                    dtUSetting = ds.Tables[1];
                    if (dtMST.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtMST.Rows)
                        {
                            var item = new SettlementType
                            {
                                ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                SetTType = dr["_Type"] is DBNull ? string.Empty : dr["_Type"].ToString(),
                                Remark = dr["_Remark"] is DBNull ? string.Empty : dr["_Remark"].ToString(),
                            };
                            settlementType.Add(item);
                        }
                    }
                    if (dtUSetting.Rows.Count > 0)
                    {
                        userwiseSettSetting.UserID = dtUSetting.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dtUSetting.Rows[0]["_ID"]);
                        userwiseSettSetting.MTRWSettleType = dtUSetting.Rows[0]["_MTRWSettleType"] is DBNull ? 0 : Convert.ToInt32(dtUSetting.Rows[0]["_MTRWSettleType"]);
                        userwiseSettSetting.MTRWSettleTypeMB = dtUSetting.Rows[0]["_MTRWSettleTypeMB"] is DBNull ? 0 : Convert.ToInt32(dtUSetting.Rows[0]["_MTRWSettleTypeMB"]);
                        userwiseSettSetting.IsOWSettleAsBank = dtUSetting.Rows[0]["_IsOWSettleAsBank"] is DBNull ? false : Convert.ToBoolean(dtUSetting.Rows[0]["_IsOWSettleAsBank"]);
                        userwiseSettSetting.MTRWSettleTypeRemark = dtUSetting.Rows[0]["_MTRWSettleTypeRemark"] is DBNull ? string.Empty : dtUSetting.Rows[0]["_MTRWSettleTypeRemark"].ToString();
                        userwiseSettSetting.MTRWSettleTypeMBRemark = dtUSetting.Rows[0]["_MTRWSettleTypeMBRemark"] is DBNull ? string.Empty : dtUSetting.Rows[0]["_MTRWSettleTypeMBRemark"].ToString();
                        userwiseSettSetting.AEPSType = dtUSetting.Rows[0]["_AEPSType"] is DBNull ? string.Empty : dtUSetting.Rows[0]["_AEPSType"].ToString();
                        userwiseSettSetting.MINIType = dtUSetting.Rows[0]["_MiniType"] is DBNull ? string.Empty : dtUSetting.Rows[0]["_MiniType"].ToString();
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetSettlementSetting";
    }

}
