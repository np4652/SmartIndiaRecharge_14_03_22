using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMNPClaim : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMNPClaim(IDAL dal) => _dal = dal;
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
                                Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                                MobileNo = row["_MobileNo"] is DBNull ? string.Empty : row["_MobileNo"].ToString(),
                                UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                                VerifyStatus = row["_Status"] is DBNull ? 0 : Convert.ToInt32(row["_Status"]),
                                RegistrationDate = row["_RegDate"] is DBNull ? string.Empty : row["_RegDate"].ToString(),
                                FRCDate = row["_FRCDate"] is DBNull ? string.Empty : row["_FRCDate"].ToString(),
                                DemoInput = row["_Demo"] is DBNull ? string.Empty : row["_Demo"].ToString(),
                                MobileMNP = row["_MobileMNP"] is DBNull ? string.Empty : row["_MobileMNP"].ToString(),
                                CutomerMObile = row["CutomerNumber"] is DBNull ? string.Empty : row["CutomerNumber"].ToString(),
                                ReferenceID = row["_ReferenceID"] is DBNull ? string.Empty : row["_ReferenceID"].ToString(),
                                 FRCType = row["_FRCType"] is DBNull ? string.Empty : row["_FRCType"].ToString(),
                                FRCDemoNumber = row["_FRCDemoNumber"] is DBNull ? string.Empty : row["_FRCDemoNumber"].ToString(),
                                FRCDoneDate = row["_FRCDoneDate"] is DBNull ? string.Empty : row["_FRCDoneDate"].ToString(),
                                Amount = row["_Amount"] is DBNull ? 0 : Convert.ToDecimal(row["_Amount"]),
                                ClaimAmountDate = row["ClaimAmtDate"] is DBNull ? string.Empty : row["ClaimAmtDate"].ToString(),
                                Operatorname = row["_Operatorname"] is DBNull ? string.Empty : row["_Operatorname"].ToString(),
                                Remark = row["_Remark"] is DBNull ? string.Empty : row["_Remark"].ToString(),
                               
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
                                Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString(),
                                MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString(),
                                UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]),
                                VerifyStatus = dt.Rows[0]["_Status"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Status"]),
                                RegistrationDate = dt.Rows[0]["_RegDate"] is DBNull ? string.Empty : dt.Rows[0]["_RegDate"].ToString(),
                                FRCDate = dt.Rows[0]["_FRCDate"] is DBNull ? string.Empty : dt.Rows[0]["_FRCDate"].ToString(),
                                DemoInput = dt.Rows[0]["_Demo"] is DBNull ? string.Empty : dt.Rows[0]["_Demo"].ToString(),
                                MobileMNP = dt.Rows[0]["_MobileMNP"] is DBNull ? string.Empty : dt.Rows[0]["_MobileMNP"].ToString(),
                                CutomerMObile = dt.Rows[0]["CutomerNumber"] is DBNull ? string.Empty : dt.Rows[0]["CutomerNumber"].ToString(),
                                ReferenceID = dt.Rows[0]["_ReferenceID"] is DBNull ? string.Empty : dt.Rows[0]["_ReferenceID"].ToString(),
                                FRCType = dt.Rows[0]["_FRCType"] is DBNull ? string.Empty : dt.Rows[0]["_FRCType"].ToString(),
                                FRCDemoNumber = dt.Rows[0]["_FRCDemoNumber"] is DBNull ? string.Empty : dt.Rows[0]["_FRCDemoNumber"].ToString(),
                                FRCDoneDate = dt.Rows[0]["_FRCDoneDate"] is DBNull ? string.Empty : dt.Rows[0]["_FRCDoneDate"].ToString(),
                                Amount = dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Amount"]),
                                ClaimAmountDate = dt.Rows[0]["ClaimAmtDate"] is DBNull ? string.Empty : dt.Rows[0]["ClaimAmtDate"].ToString(),
                                Operatorname = dt.Rows[0]["_Operatorname"] is DBNull ? string.Empty : dt.Rows[0]["_Operatorname"].ToString(),
                                Remark = dt.Rows[0]["_Remark"] is DBNull ? string.Empty : dt.Rows[0]["_Remark"].ToString(),
                                

                            };
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetMNPClaim";
    }
}
