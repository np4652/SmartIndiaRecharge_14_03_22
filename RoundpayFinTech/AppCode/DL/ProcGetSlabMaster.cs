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
    public class ProcGetSlabMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSlabMaster(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@SlabID",req.CommonInt)
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
                                Slab = row["_Slab"] is DBNull ? "" : row["_Slab"].ToString(),
                                EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                                ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                                IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                                IsRealSlab = row["_IsRealSlab"] is DBNull ? false : Convert.ToBoolean(row["_IsRealSlab"]),
                                IsAdminDefined = row["_IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(row["_IsAdminDefined"]),
                                IsSigunupSlabID = row["IsSigunupSlabID"] is DBNull ? false : Convert.ToBoolean(row["IsSigunupSlabID"]),
                                IsB2B = row["_IsB2B"] is DBNull ? false : Convert.ToBoolean(row["_IsB2B"]),
                                DMRModelID = Convert.ToInt16(row["_DMRModelID"] is DBNull ? 0 : row["_DMRModelID"]),
                                DMRModel = row["_DMRModel"] is DBNull ? string.Empty : row["_DMRModel"].ToString(),
                                TotalUser = row["_TotalUser"] is DBNull ? 0 : Convert.ToInt32(row["_TotalUser"]),
                                IsMultiLevel = row["_IsMultiLevel"] is DBNull ? 0 : Convert.ToInt16(row["_IsMultiLevel"])
                            };
                            res.Add(slabMaster);
                        }
                    }
                    else
                    {
                        return new SlabMaster
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]),
                            Slab = dt.Rows[0]["_Slab"] is DBNull ? "" : dt.Rows[0]["_Slab"].ToString(),
                            EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString(),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                            IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                            Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString(),
                            IsRealSlab = dt.Rows[0]["_IsRealSlab"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRealSlab"]),
                            IsAdminDefined = dt.Rows[0]["_IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAdminDefined"]),
                            IsSigunupSlabID = dt.Rows[0]["IsSigunupSlabID"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsSigunupSlabID"]),
                            IsB2B = dt.Rows[0]["_IsB2B"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsB2B"]),
                            DMRModelID = Convert.ToInt16(dt.Rows[0]["_DMRModelID"] is DBNull ? 0 : dt.Rows[0]["_DMRModelID"]),
                            DMRModel = dt.Rows[0]["_DMRModel"] is DBNull ? string.Empty : dt.Rows[0]["_DMRModel"].ToString(),
                            TotalUser = dt.Rows[0]["_TotalUser"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TotalUser"])              
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

        public string GetName() => "proc_GetSlabMaster";
    }
}
