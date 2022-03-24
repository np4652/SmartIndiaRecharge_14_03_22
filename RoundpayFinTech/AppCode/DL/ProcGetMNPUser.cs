using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMNPUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMNPUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (MNPUser)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@LoginID", req.UserID),
                new SqlParameter("@VerifyStatus", req.VerifyStatus)
            };
            //var resp = new PartnerDetailsResp();
            var resp = new List<MNPUser>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.ID == 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var partnerCreate = new MNPUser
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                                VerifyStatus = row["_VerifyStatus"] is DBNull ? 0 : Convert.ToInt32(row["_VerifyStatus"]),
                                Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                                MobileNo = row["_MobileNo"] is DBNull ? string.Empty : row["_MobileNo"].ToString(),
                                UserName = row["_UserName"] is DBNull ? string.Empty : row["_UserName"].ToString(),
                                Password = row["_Password"] is DBNull ? string.Empty : row["_Password"].ToString(),
                                Operatorname = row["_Operatorname"] is DBNull ? string.Empty : row["_Operatorname"].ToString(),
                                Remark = row["_Remark"] is DBNull ? string.Empty : row["_Remark"].ToString(),
                                DemoInput = row["_Demo"] is DBNull ? string.Empty : row["_Demo"].ToString(),
                            };
                            resp.Add(partnerCreate);
                        }
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            return new MNPUser
                            {
                                ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                                UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]),
                                VerifyStatus = dt.Rows[0]["_VerifyStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_VerifyStatus"]),
                                Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString(),
                                MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString(),
                                UserName = dt.Rows[0]["_UserName"] is DBNull ? string.Empty : dt.Rows[0]["_UserName"].ToString(),
                                Password = dt.Rows[0]["_Password"] is DBNull ? string.Empty : dt.Rows[0]["_Password"].ToString(),
                                Operatorname = dt.Rows[0]["_Operatorname"] is DBNull ? string.Empty : dt.Rows[0]["_Operatorname"].ToString(),
                                Remark = dt.Rows[0]["_Remark"] is DBNull ? string.Empty : dt.Rows[0]["_Remark"].ToString(),
                                DemoInput = dt.Rows[0]["_Demo"] is DBNull ? string.Empty : dt.Rows[0]["_Demo"].ToString(),

                            };
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetMNPUser";
    }
}
