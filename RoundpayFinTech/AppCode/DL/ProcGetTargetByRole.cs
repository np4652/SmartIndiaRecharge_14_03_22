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
    public class ProcGetTargetByRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetTargetByRole(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetTargetByRole";

        public object Call(object obj)
        {
            var req = (TargetModelReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@SlabID", req.Detail.SlabID),
                new SqlParameter("@OID", req.Detail.OID),
                //new SqlParameter("@TargetTypeID", req.Detail.TargetTypeID),
                new SqlParameter("@RoleID", req.Detail.RoleID)
            };
            TargetModel res = new TargetModel();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]);
                        res.SlabID = dr["_SlabID"] is DBNull ? 0 : Convert.ToInt32(dr["_SlabID"]);
                        res.OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]);
                        res.RoleID = dr["_RoleID"] is DBNull ? 0 : Convert.ToInt32(dr["_RoleID"]);
                        res.AmtType = dr["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_AmtType"]);
                        res.Comm = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]);
                        res.Target = dr["_Target"] is DBNull ? 0 : Convert.ToInt32(dr["_Target"]);
                        res.IsEarned = dr["_IsEarned"] is DBNull ? false : Convert.ToBoolean(dr["_IsEarned"]);
                        res.IsGift = dr["_IsGift"] is DBNull ? false : Convert.ToBoolean(dr["_IsGift"]);
                        res.ModifyDate = dr["_ModifyDate"] is DBNull ? "" : Convert.ToString(dr["_ModifyDate"]);
                        res.TargetTypeID = dr["_TargetTypeID"] is DBNull ? 0 : Convert.ToInt32(dr["_TargetTypeID"]);
                        res.IsHikeOnEarned = dr["_IsHikeOnEarned"] is DBNull ? false : Convert.ToBoolean(dr["_IsHikeOnEarned"]);
                        res.HikePer = dr["_HikePer"] is DBNull ? 0 : Convert.ToDecimal(dr["_HikePer"]);
                        string[] ext = { ".png", ".jpg", ".jpeg" };
                        foreach (string s in ext)
                        {
                            string fileName = "Gift_" + res.RoleID + "_" + res.OID + "_" + res.SlabID + s;
                            string file = DOCType.GiftImgPath + fileName;
                            if (File.Exists(file))
                            {
                                res.ImgaePath = "/Image/GiftImage/" + fileName;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
    }
}
