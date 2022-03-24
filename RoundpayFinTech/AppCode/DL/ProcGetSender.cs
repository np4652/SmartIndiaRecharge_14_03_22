using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSender : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSender(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            string MobileNo = (string)obj;
            var res = new SenderRequest();
            SqlParameter[] param = {
                new SqlParameter("@MobileNo",MobileNo),
            };

            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
                    res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString();
                    res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? "" : dt.Rows[0]["_Pincode"].ToString();
                    res.Address = dt.Rows[0]["_Address"] is DBNull ? "" : dt.Rows[0]["_Address"].ToString();
                    res.City = dt.Rows[0]["_City"] is DBNull ? "" : dt.Rows[0]["_City"].ToString();
                    res.StateID = Convert.ToInt32(dt.Rows[0]["_StateID"]);
                    res.AadharNo = dt.Rows[0]["_AadhaarNo"] is DBNull ? "" : dt.Rows[0]["_AadhaarNo"].ToString();
                    res.ReffID = dt.Rows[0]["_ReffID"] is DBNull ? "" : dt.Rows[0]["_ReffID"].ToString();
                    res.RequestNo = dt.Rows[0]["_RequestNo"] is DBNull ? "" : dt.Rows[0]["_RequestNo"].ToString();
                    res.OTP = dt.Rows[0]["_OTP"] is DBNull ? "" : dt.Rows[0]["_OTP"].ToString();
                    res.Dob = dt.Rows[0]["_Dob"] is DBNull ? "" : dt.Rows[0]["_Dob"].ToString();
                    res._VerifyStatus = dt.Rows[0]["_VerifyStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_VerifyStatus"]);
                    res.IsNotCheckLimit = dt.Rows[0]["_IsNotCheckLimit"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsNotCheckLimit"]);
                }
            }
            catch (Exception)
            {
            }
            return res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "select * from tbl_Sender where _MobileNo=@MobileNo";
        }
    }
}
