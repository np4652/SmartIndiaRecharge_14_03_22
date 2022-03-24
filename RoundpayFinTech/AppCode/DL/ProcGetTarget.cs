using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTarget : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetTarget(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetTargetByOID";

        public object Call(object obj)
        {
            var req = (TargetModelReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@SlabID", req.Detail.SlabID),
                new SqlParameter("@OID", req.Detail.OID),
            };
            var resList = new List<TargetModel>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var res = new TargetModel
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            RoleID = dr["_RoleID"] is DBNull ? 0 : Convert.ToInt32(dr["_RoleID"]),
                            RoleName = dr["_RoleName"] is DBNull ? "" : Convert.ToString(dr["_RoleName"]),
                            SlabID = dr["_SlabID"] is DBNull ? 0 : Convert.ToInt32(dr["_SlabID"]),
                            OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                            OpName = dr["_OpName"].ToString(),
                            OpTypeName = dr["_OpTypeName"].ToString(),
                            AmtType = dr["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_AmtType"]),
                            Comm = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]),
                            Target = dr["_Target"] is DBNull ? 0 : Convert.ToInt32(dr["_Target"]),
                            IsEarned = dr["_IsEarned"] is DBNull ? false : Convert.ToBoolean(dr["_IsEarned"]),
                            IsGift = dr["_IsGift"] is DBNull ? false : Convert.ToBoolean(dr["_IsGift"]),
                            ModifyDate = dr["_ModifyDate"] is DBNull ? "" : Convert.ToString(dr["_ModifyDate"]),
                            TargetTypeID = Convert.ToInt32(dr["_TargetTypeID"]),
                            HikePer = dr["_HikePer"] is DBNull ? 0 : Convert.ToDecimal(dr["_HikePer"]),
                            IsHikeOnEarned = dr["_IsHikeOnEarned"] is DBNull ? false : Convert.ToBoolean(dr["_IsHikeOnEarned"])
                        };
                        string[] ext = {".png",".jpg",".jpeg"};
                        foreach(string s in ext)
                        {
                            string fileName = "Gift_" + res.RoleID + "_" + res.OID + "_" + res.SlabID + s;
                            string file = DOCType.GiftImgPath + fileName;
                            if (File.Exists(file))
                            {
                                res.ImgaePath = "/Image/GiftImage/" + fileName;
                                break;
                            }
                        }
                        resList.Add(res);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return resList;
        }

        public object Call() => throw new NotImplementedException();
    }
}
