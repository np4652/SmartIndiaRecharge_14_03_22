using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAPICommissionRange : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAPICommissionRange(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@APIID",req.CommonInt),
                new SqlParameter("@CommSettingType",req.CommonInt2),
                new SqlParameter("@OpTypeID",req.CommonInt3)
            };
            List<RangeCommission> slabCommissions = new List<RangeCommission>();
            RangeDetailModel resp = new RangeDetailModel
            {
                IsAdminDefined = false,
                SlabDetails = slabCommissions,
                ParentSlabDetails = slabCommissions,
                SlabID = req.CommonInt
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var slabCommission = new RangeCommission
                        {
                            SlabID = row["SlabID"] is DBNull ? 0 : Convert.ToInt32(row["SlabID"]),
                            OID = row["OID"] is DBNull ? 0 : Convert.ToInt32(row["OID"]),
                            Operator = row["Operator"] is DBNull ? "" : row["Operator"].ToString(),
                            SPKey = row["SPKey"] is DBNull ? "" : row["SPKey"].ToString(),
                            IsBilling = row["_IsBilling"] is DBNull ? false : Convert.ToBoolean(row["_IsBilling"]),
                            IsBBPS = row["IsBBPS"] is DBNull ? false : Convert.ToBoolean(row["IsBBPS"]),
                            OpType = row["OpType"] is DBNull ? 0 : Convert.ToInt32(row["OpType"]),
                            OperatorType = row["OperatorType"] is DBNull ? "" : row["OperatorType"].ToString(),
                            SlabDetailID = row["SDID"] is DBNull ? 0 : Convert.ToInt32(row["SDID"]),
                            Comm = row["Commission"] is DBNull ? 0 : Convert.ToDecimal(row["Commission"]),
                            CommType = row["CommType"] is DBNull ? 0 : Convert.ToInt32(row["CommType"]),
                            AmtType = row["AmtType"] is DBNull ? 0 : Convert.ToInt32(row["AmtType"]),
                            ModifyDate = row["ModifyDate"] is DBNull ? "" : row["ModifyDate"].ToString(),
                            Min = row["_Min"] is DBNull ? 0 : Convert.ToInt32(row["_Min"]),
                            Max = row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            BusinessModel = row["_BusinessModel"] is DBNull ? "" : row["_BusinessModel"].ToString(),
                            MinRange = row["_MinRange"] is DBNull ? 0 : Convert.ToInt32(row["_MinRange"]),
                            MaxRange = row["_MaxRange"] is DBNull ? 0 : Convert.ToInt32(row["_MaxRange"]),
                            MaxComm = row["_MaxComm"] is DBNull ? 0 : Convert.ToDecimal(row["_MaxComm"]),
                            FixedCharge = row["_FixedCharge"] is DBNull ? 0 : Convert.ToInt32(row["_FixedCharge"]),
                            RangeId = row["RangeId"] is DBNull ? 0 : Convert.ToInt32(row["RangeId"]),
                            DMRModelID = row["_DMRModelID"] is DBNull ? 0 : Convert.ToInt32(row["_DMRModelID"])
                        };
                       
                        slabCommissions.Add(slabCommission);
                    }
                    resp.SlabDetails = slabCommissions;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAPICommission_Range";
    }
}
