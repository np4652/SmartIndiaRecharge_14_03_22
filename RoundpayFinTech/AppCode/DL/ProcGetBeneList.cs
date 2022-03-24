using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBeneList:IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetBeneList(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            WalletRequest _req = (WalletRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginId", _req.LoginID),
            };
            List<WalletRequest> _lst = new List<WalletRequest>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        WalletRequest _res = new WalletRequest()
                        {
                            UserRoleId = dr["_UserRoleId"] is DBNull ? string.Empty : Convert.ToString(dr["_UserRoleId"]),
                            UserName = dr["_Name"] is DBNull ? string.Empty : Convert.ToString(dr["_Name"]),
                            Mobile = dr["_MobileNo"] is DBNull ? string.Empty : Convert.ToString(dr["_MobileNo"]),
                            BankName = dr["_BankName"] is DBNull ? string.Empty : Convert.ToString(dr["_BankName"]),
                            IFSC = dr["_IFSC"] is DBNull ? string.Empty : Convert.ToString(dr["_IFSC"]),
                            AccountHolder = dr["_AccountHolder"] is DBNull ? string.Empty : Convert.ToString(dr["_AccountHolder"]),
                            AccountNumber = dr["_AccountNumber"] is DBNull ? string.Empty : Convert.ToString(dr["_AccountNumber"]),
                            Address = dr["_Address"] is DBNull ? string.Empty : Convert.ToString(dr["_Address"]),
                            City = dr["_City"] is DBNull ? string.Empty : Convert.ToString(dr["_City"]),
                            State = dr["StateName"] is DBNull ? string.Empty : Convert.ToString(dr["StateName"]),
                            Email = dr["_EmailID"] is DBNull ? string.Empty : Convert.ToString(dr["_EmailID"]),
                            Pincode = dr["_Pincode"] is DBNull ? string.Empty : Convert.ToString(dr["_Pincode"])
                        };

                        if (_req.ID > 0)
                        {
                            return _res;
                        }
                        else
                        {
                            _lst.Add(_res);
                        }
                    }
                }

            }
            catch (Exception er)
            { }
            return _lst;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_GetBeneList";
        }
    }
}

