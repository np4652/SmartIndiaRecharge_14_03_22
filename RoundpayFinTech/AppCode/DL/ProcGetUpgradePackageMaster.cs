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
    public class ProcGetUpgradePackageMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUpgradePackageMaster(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req)
            };
            var res = new List<SlabMaster>();
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
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
                        };
                        res.Add(slabMaster);
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
                    LoginTypeID = 1,
                    UserId = req
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "select * from Master_Package m where m._PackageCost >=(select MAX(mp._PackageCost) from tbl_Users_Package p ,Master_Package mp  where mp._ID=p._PackageID and _UserID=@UserID) order by _ID ";
    }
}