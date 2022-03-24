using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTargetAchievedTillDate : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetTargetAchievedTillDate(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@IsTotal",req.IsListType),
                new SqlParameter("@UserID",req.CommonInt)
            };
            var res = new List<TargetAchieved>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) != ErrorCodes.Minus1)
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            var data = new TargetAchieved
                            {
                                SID = item["_SID"] is DBNull ? 0 : Convert.ToInt32(item["_SID"]),
                                Service = item["_Service"] is DBNull ? string.Empty : item["_Service"].ToString(),
                                Target = item["_SetTarget"] is DBNull ? 0 : Convert.ToDecimal(item["_SetTarget"]),
                                TargetTillDate = item["_TargetTillDate"] is DBNull ? 0 : Convert.ToDecimal(item["_TargetTillDate"]),
                                TodaySale = item["_TodaySale"] is DBNull ? 0 : Convert.ToInt32(item["_TodaySale"]),
                                RoleID = item["RoleID"] is DBNull ? 0 : Convert.ToInt32(item["RoleID"]),
                                OID = item["ServiceID"] is DBNull ? 0 : Convert.ToInt32(item["ServiceID"]),//in table it's _OID
                                SlabID = item["SlabID"] is DBNull ? 0 : Convert.ToInt32(item["SlabID"]),
                                IsGift = item["IsGift"] is DBNull ? false : Convert.ToBoolean(item["IsGift"])
                            };
                            string[] ext = { ".png", ".jpg", ".jpeg" };
                            foreach (string s in ext)
                            {
                                string fileName = "Gift_" + data.RoleID + "_" + data.OID + "_" + data.SlabID + s;
                                string file = DOCType.GiftImgPath + fileName;
                                if (File.Exists(file))
                                {
                                    data.ImgaePath = "/Image/GiftImage/" + fileName;
                                    break;
                                }
                            }
                            res.Add(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetTargetAchievedTillDate";
    }
}
