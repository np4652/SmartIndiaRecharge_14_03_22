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
    public class ProcGetPackageMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPackageMaster(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@PackageID",req.CommonInt)
            };
            var res = new List<SlabMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == -1)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var slabMaster = new SlabMaster
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                Slab = row["_Package"] is DBNull ? string.Empty : row["_Package"].ToString(),
                                EntryDate = row["_EntryDate"] is DBNull ? string.Empty : row["_EntryDate"].ToString(),
                                ModifyDate = row["_ModifyDate"] is DBNull ? string.Empty : row["_ModifyDate"].ToString(),
                                IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                Remark = row["_Remark"] is DBNull ? string.Empty : row["_Remark"].ToString(),
                                IsDefault = row["_IsDefault"] is DBNull ? false : Convert.ToBoolean(row["_IsDefault"]),
                                PackageCost = row["_PackageCost"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageCost"]),
                                IsShowMore = row["_IsShowMore"] is DBNull ? false : Convert.ToBoolean(row["_IsShowMore"]),
                                SelfAssigned = row["_SelfAssigned"] is DBNull ? false : Convert.ToBoolean(row["_SelfAssigned"]),
                            };
                            res.Add(slabMaster);
                        }
                    }
                    else
                    {
                        return new SlabMaster
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]),
                            Slab = dt.Rows[0]["_Package"] is DBNull ? "" : dt.Rows[0]["_Package"].ToString(),
                            EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString(),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                            IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                            Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString(),
                            IsDefault = dt.Rows[0]["_IsDefault"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDefault"]),
                            PackageCost = dt.Rows[0]["_PackageCost"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_PackageCost"]),
                            IsShowMore = dt.Rows[0]["_IsShowMore"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsShowMore"]),
                            SelfAssigned = dt.Rows[0]["_SelfAssigned"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_SelfAssigned"]),
                        };
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
            if (req.CommonInt == -1)
                return res;
            else
                return new SlabMaster();
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetPackageMaster";
    }
}