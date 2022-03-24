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
    public class ProcGetSlabDetailRange : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSlabDetailRange(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.CommonInt),
                new SqlParameter("@CommSettingType",req.CommonInt2),
                new SqlParameter("@OpTypeID",req.CommonInt3 == 0 ? 0: req.CommonInt3)
            };
            List<RangeCommission> slabCommissions = new List<RangeCommission>();
            List<RoleMaster> Roles = new List<RoleMaster>();
            RangeDetailModel resp = new RangeDetailModel
            {
                IsAdminDefined = false,
                SlabDetails = slabCommissions,
                ParentSlabDetails = slabCommissions,
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
                        dtParent = ds.Tables.Count == 2 ? ds.Tables[1] : dtParent;
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
                            MaxComm = row["_MaxComm"] is DBNull ? 0 : Convert.ToInt32(row["_MaxComm"]),
                            FixedCharge = row["_FixedCharge"] is DBNull ? 0 : Convert.ToInt32(row["_FixedCharge"]),
                            RangeId = row["RangeId"] is DBNull ? 0 : Convert.ToInt32(row["RangeId"]),
                            DMRModelID = row["_DMRModelID"] is DBNull ? 0 : Convert.ToInt32(row["_DMRModelID"]),
                            RComm = row["RCommission"] is DBNull ? 0 : Convert.ToDecimal(row["RCommission"]),
                            RCommType = row["RCommType"] is DBNull ? 0 : Convert.ToInt32(row["RCommType"]),
                            RAmtType = row["RAmtType"] is DBNull ? 0 : Convert.ToInt32(row["RAmtType"]),
                            RMaxComm = row["_RMaxComm"] is DBNull ? 0 : Convert.ToInt32(row["_RMaxComm"]),
                            RFixedCharge = row["_RFixedCharge"] is DBNull ? 0 : Convert.ToInt32(row["_RFixedCharge"])
                        };
                        if (dt.Columns.Contains("_RoleID"))
                        {
                            slabCommission.RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]);
                            if (!resp.IsAdminDefined)
                                resp.IsAdminDefined = !resp.IsAdminDefined;
                        }
                        slabCommissions.Add(slabCommission);
                    }
                    resp.SlabDetails = slabCommissions;
                }
                if (dtParent.Rows.Count > 0 && !dtParent.Columns.Contains("Msg"))
                {
                    slabCommissions = new List<RangeCommission>();
                    if (dtParent.Columns.Contains("SlabID"))
                    {
                        foreach (DataRow row in dtParent.Rows)
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
                                MaxComm = row["_MaxComm"] is DBNull ? 0 : Convert.ToInt32(row["_MaxComm"]),
                                FixedCharge = row["_FixedCharge"] is DBNull ? 0 : Convert.ToInt32(row["_FixedCharge"]),
                                RangeId = row["RangeId"] is DBNull ? 0 : Convert.ToInt32(row["RangeId"]),
                                DMRModelID = row["_DMRModelID"] is DBNull ? 0 : Convert.ToInt32(row["_DMRModelID"]),
                                RComm = row["RCommission"] is DBNull ? 0 : Convert.ToDecimal(row["RCommission"]),
                                RCommType = row["RCommType"] is DBNull ? 0 : Convert.ToInt32(row["RCommType"]),
                                RAmtType = row["RAmtType"] is DBNull ? 0 : Convert.ToInt32(row["RAmtType"]),
                                RMaxComm = row["_RMaxComm"] is DBNull ? 0 : Convert.ToInt32(row["_RMaxComm"]),
                                RFixedCharge = row["_RFixedCharge"] is DBNull ? 0 : Convert.ToInt32(row["_RFixedCharge"])

                            };
                            if (dt.Columns.Contains("_RoleID"))
                            {
                                slabCommission.RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]);
                            }
                            slabCommissions.Add(slabCommission);
                        }
                    }
                    resp.ParentSlabDetails = slabCommissions;
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

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetSlabDetail_Range";
    }

    public class ProcGetSlabDetailDisplayLvLRange : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSlabDetailDisplayLvLRange(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var slabCommissions = new List<RangeCommission>();
            var Roles = new List<RoleMaster>();
            var resp = new RangeDetailModel
            {
                IsAdminDefined = false,
                SlabDetails = slabCommissions,
                ParentSlabDetails = slabCommissions,
                Roles = Roles,
                SlabID = req.CommonInt
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtRole = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    dtRole = ds.Tables.Count == 2 ? ds.Tables[1] : dtRole;
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        slabCommissions.Add(new RangeCommission
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
                            RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]),
                            MinRange = row["_MinRange"] is DBNull ? 0 : Convert.ToInt32(row["_MinRange"]),
                            MaxRange = row["_MaxRange"] is DBNull ? 0 : Convert.ToInt32(row["_MaxRange"]),
                            MaxComm = row["_MaxComm"] is DBNull ? 0 : Convert.ToDecimal(row["_MaxComm"]),
                            FixedCharge = row["_FixedCharge"] is DBNull ? 0 : Convert.ToDecimal(row["_FixedCharge"]),
                            RangeId = row["RangeId"] is DBNull ? 0 : Convert.ToInt32(row["RangeId"]),
                            DMRModelID = row["_DMRModelID"] is DBNull ? 0 : Convert.ToInt32(row["_DMRModelID"]),
                            RComm = row["RCommission"] is DBNull ? 0 : Convert.ToDecimal(row["RCommission"]),
                            RCommType = row["RCommType"] is DBNull ? 0 : Convert.ToInt32(row["RCommType"]),
                            RAmtType = row["RAmtType"] is DBNull ? 0 : Convert.ToInt32(row["RAmtType"]),
                            RMaxComm = row["_RMaxComm"] is DBNull ? 0 : Convert.ToInt32(row["_RMaxComm"]),
                            RFixedCharge = row["_RFixedCharge"] is DBNull ? 0 : Convert.ToInt32(row["_RFixedCharge"])
                        });
                    }
                    resp.SlabDetails = slabCommissions;
                    if (dtRole.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtRole.Rows)
                        {
                            var roleMaster = new RoleMaster
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                                Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString()
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
                    ClassName = "ProcGetSlabDetailDisplayLvLRange",
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

        public string GetName() => "proc_GetSlabDetailDisplayLvL_Range";
    }
}
