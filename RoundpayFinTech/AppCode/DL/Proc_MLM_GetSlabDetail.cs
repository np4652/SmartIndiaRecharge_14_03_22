using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class Proc_MLM_GetSlabDetail : IProcedure
    {
        private readonly IDAL _dal;

        public Proc_MLM_GetSlabDetail(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.CommonInt),
                new SqlParameter("@OpTypeID",req.CommonInt2),  
            };
            List<MLM_SlabCommission> lst_mlm_SlabCommissions = new List<MLM_SlabCommission>();
            List<RoleMaster> Roles = new List<RoleMaster>();
            MLM_SlabDetailModel resp = new MLM_SlabDetailModel
            {
                IsAdminDefined = false,
                mlmSlabDetails = lst_mlm_SlabCommissions,
                mlmParentSlabDetails = lst_mlm_SlabCommissions,
                Roles = Roles,
                SlabID = req.CommonInt
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtParent = new DataTable();
                DataTable dtRole = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    if (req.IsListType)
                        dtRole = ds.Tables.Count == 2 ? ds.Tables[1] : dtRole;
                    else
                        dtParent = ds.Tables.Count > 1 ? ds.Tables[1] : dtParent;
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var mlmslabCommission = new MLM_SlabCommission
                        {
                            //OID	Operator	SPKey	_RoleID	Commission	CommType	AmtType	SlabID
                            SlabID = row["SlabID"] is DBNull ? 0 : Convert.ToInt32(row["SlabID"]),
                            OID = row["OID"] is DBNull ? 0 : Convert.ToInt32(row["OID"]),
                            Operator = row["Operator"] is DBNull ? "" : row["Operator"].ToString(),
                            SPKey = row["SPKey"] is DBNull ? "" : row["SPKey"].ToString(),
                            IsBilling = row["_IsBilling"] is DBNull ? false : Convert.ToBoolean(row["_IsBilling"]),
                            IsBBPS = row["IsBBPS"] is DBNull ? false : Convert.ToBoolean(row["IsBBPS"]),
                            OpType = row["OpType"] is DBNull ? 0 : Convert.ToInt32(row["OpType"]),
                            OperatorType = row["OperatorType"] is DBNull ? "" : row["OperatorType"].ToString(),
                            Comm = row["Commission"] is DBNull ? 0 : Convert.ToDecimal(row["Commission"]),
                            CommType = row["CommType"] is DBNull ? 0 : Convert.ToInt32(row["CommType"]),
                            AmtType = row["AmtType"] is DBNull ? 0 : Convert.ToInt32(row["AmtType"]),
                            ModifyDate = row["ModifyDate"] is DBNull ? "" : row["ModifyDate"].ToString(),
                            Min = row["_Min"] is DBNull ? 0 : Convert.ToInt32(row["_Min"]),
                            Max = row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            BusinessModel = row["_BusinessModel"] is DBNull ? "" : row["_BusinessModel"].ToString()
                        };
                        if (dt.Columns.Contains("_LevelId"))
                        {
                            mlmslabCommission.LevelID = row["_LevelId"] is DBNull ? 0 : Convert.ToInt32(row["_LevelId"]);
                            if (!resp.IsAdminDefined)
                                resp.IsAdminDefined = !resp.IsAdminDefined;
                        }
                        lst_mlm_SlabCommissions.Add(mlmslabCommission);
                    }
                    resp.mlmSlabDetails = lst_mlm_SlabCommissions;
                }

                if (dtParent.Rows.Count > 0 && !dtParent.Columns.Contains("Msg"))
                {
                    lst_mlm_SlabCommissions = new List<MLM_SlabCommission>();

                    if (dtParent.Columns.Contains("SlabID"))
                    {
                        foreach (DataRow row in dtParent.Rows)
                        {
                            var mlm_slabCommission = new MLM_SlabCommission
                            {
                                SlabID = row["SlabID"] is DBNull ? 0 : Convert.ToInt32(row["SlabID"]),
                                OID = row["OID"] is DBNull ? 0 : Convert.ToInt32(row["OID"]),
                                Operator = row["Operator"] is DBNull ? "" : row["Operator"].ToString(),
                                SPKey = row["SPKey"] is DBNull ? "" : row["SPKey"].ToString(),
                                IsBilling = row["_IsBilling"] is DBNull ? false : Convert.ToBoolean(row["_IsBilling"]),
                                IsBBPS = row["IsBBPS"] is DBNull ? false : Convert.ToBoolean(row["IsBBPS"]),
                                OpType = row["OpType"] is DBNull ? 0 : Convert.ToInt32(row["OpType"]),
                                OperatorType = row["OperatorType"] is DBNull ? "" : row["OperatorType"].ToString(),
                                Comm = row["Commission"] is DBNull ? 0 : Convert.ToDecimal(row["Commission"]),
                                CommType = row["CommType"] is DBNull ? 0 : Convert.ToInt32(row["CommType"]),
                                AmtType = row["AmtType"] is DBNull ? 0 : Convert.ToInt32(row["AmtType"]),
                                ModifyDate = row["ModifyDate"] is DBNull ? "" : row["ModifyDate"].ToString(),
                                Min = row["_Min"] is DBNull ? 0 : Convert.ToInt32(row["_Min"]),
                                Max = row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                                IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                BusinessModel = row["_BusinessModel"] is DBNull ? "" : row["_BusinessModel"].ToString()
                            };
                            if (dt.Columns.Contains("_LevelId"))
                            {
                                mlm_slabCommission.LevelID = row["_LevelId"] is DBNull ? 0 : Convert.ToInt32(row["_LevelId"]);
                            }
                            lst_mlm_SlabCommissions.Add(mlm_slabCommission);
                        }
                    }
                    resp.mlmParentSlabDetails = lst_mlm_SlabCommissions;
                }
                if (resp.IsAdminDefined)
                {
                    if (dtRole.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtRole.Rows)
                        {
                            var roleMaster = new RoleMaster
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                                Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            };
                            Roles.Add(roleMaster);
                        }
                        resp.Roles = Roles;
                    }
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "MLM_Proc_GetSlabDetail";
    }
}
