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
    public class ProcGetVASPackageMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVASPackageMaster(IDAL dal) => _dal = dal;
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
                    if (dt.Rows[0][0].ToString() != "-1")
                    {
                        if (req.CommonInt == -1)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                var slabMaster = new SlabMaster
                                {
                                    ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                    Slab = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                                    IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                    PackageCost = row["_Charge"] is DBNull ? 0 : Convert.ToDecimal(row["_Charge"]),
                                    EntryDate = row["_EntryDate"] is DBNull ? string.Empty : row["_EntryDate"].ToString(),
                                    ModifyDate = row["_ModifyDate"] is DBNull ? string.Empty : row["_ModifyDate"].ToString(),
                                    ValidityInDays = row["_ValidityInDays"] is DBNull ? 0 : Convert.ToInt32(row["_ValidityInDays"]),
                                    DailyHitCount = row["_DailyHitLimit"] is DBNull ? 0 : Convert.ToInt32(row["_DailyHitLimit"]),
                                    Remark = row["_Remark"] is DBNull ? string.Empty : row["_Remark"].ToString(),
                                };
                                res.Add(slabMaster);
                            }
                        }
                        else
                        {
                            return new SlabMaster
                            {
                                ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]),
                                Slab = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString(),
                                IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                                PackageCost = dt.Rows[0]["_Charge"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Charge"]),
                                EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString(),
                                ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                                ValidityInDays = dt.Rows[0]["_ValidityInDays"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ValidityInDays"]),
                                DailyHitCount = dt.Rows[0]["_DailyHitLimit"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_DailyHitLimit"]),
                                Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString()
                            };
                        }
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

        public string GetName() => "proc_GetVASPackageMaster";
    }
}