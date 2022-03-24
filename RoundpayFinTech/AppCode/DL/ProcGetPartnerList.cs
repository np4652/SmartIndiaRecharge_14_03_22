using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPartnerList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPartnerList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (PartnerCreate)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@UserID", req.UserID),
                new SqlParameter("@Status", req.Status)
            };
            //var resp = new PartnerDetailsResp();
            var resp = new List<PartnerCreate>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.ID == 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var partnerCreate = new PartnerCreate
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                APIUserName = row["APIUserName"] is DBNull ? string.Empty : row["APIUserName"].ToString(),
                                UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                                Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                                FatherName = row["_FatherName"] is DBNull ? string.Empty : row["_FatherName"].ToString(),
                                DOB = Convert.ToDateTime(row["_DOB"]),
                                OutletName = row["_OutletName"] is DBNull ? string.Empty : row["_OutletName"].ToString(),
                                EmailID = row["_EmailID"] is DBNull ? string.Empty : row["_EmailID"].ToString(),
                                MobileNo = row["_MobileNo"] is DBNull ? string.Empty : row["_MobileNo"].ToString(),
                                PAN = row["_PAN"] is DBNull ? string.Empty : row["_PAN"].ToString(),
                                AADHAR = row["_AADHAR"] is DBNull ? string.Empty : row["_AADHAR"].ToString(),
                                CompanyPAN = row["_CompanyPAN"] is DBNull ? string.Empty : row["_CompanyPAN"].ToString(),
                                GSTIN = row["_GSTIN"] is DBNull ? string.Empty : row["_GSTIN"].ToString(),
                                AuthPersonName = row["_AuthPersonName"] is DBNull ? string.Empty : row["_AuthPersonName"].ToString(),
                                AuthPersonAADHAR = row["_AuthPersonAADHAR"] is DBNull ? string.Empty : row["_AuthPersonAADHAR"].ToString(),
                                CurrentAccountNo = row["_CurrentAccountNo"] is DBNull ? string.Empty : row["_CurrentAccountNo"].ToString(),
                                Address = row["_Address"] is DBNull ? string.Empty : row["_Address"].ToString(),
                                Block = row["_Block"] is DBNull ? string.Empty : row["_Block"].ToString(),
                                City = row["CityName"] is DBNull ? string.Empty : row["CityName"].ToString(),
                                District = row["Districtname"] is DBNull ? string.Empty : row["Districtname"].ToString(),
                                State = row["Statename"] is DBNull ? string.Empty : row["Statename"].ToString(),
                                Pincode = row["_Pincode"] is DBNull ? string.Empty : row["_Pincode"].ToString(),
                                IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                Status = row["_Status"] is DBNull ? 1 : Convert.ToInt32(row["_Status"])
                            };
                            resp.Add(partnerCreate);
                        }
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            return new PartnerCreate
                            {
                                ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                                APIUserName = dt.Rows[0]["APIUserName"] is DBNull ? string.Empty : dt.Rows[0]["APIUserName"].ToString(),
                                UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]),
                                Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString(),
                                FatherName = dt.Rows[0]["_FatherName"] is DBNull ? string.Empty : dt.Rows[0]["_FatherName"].ToString(),

                                DOB = Convert.ToDateTime(dt.Rows[0]["_DOB"]),
                                OutletName = dt.Rows[0]["_OutletName"] is DBNull ? string.Empty : dt.Rows[0]["_OutletName"].ToString(),
                                EmailID = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString(),
                                MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString(),
                                PAN = dt.Rows[0]["_PAN"] is DBNull ? string.Empty : dt.Rows[0]["_PAN"].ToString(),
                                AADHAR = dt.Rows[0]["_AADHAR"] is DBNull ? string.Empty : dt.Rows[0]["_AADHAR"].ToString(),
                                CompanyPAN = dt.Rows[0]["_CompanyPAN"] is DBNull ? string.Empty : dt.Rows[0]["_CompanyPAN"].ToString(),
                                GSTIN = dt.Rows[0]["_GSTIN"] is DBNull ? string.Empty : dt.Rows[0]["_GSTIN"].ToString(),
                                AuthPersonName = dt.Rows[0]["_AuthPersonName"] is DBNull ? string.Empty : dt.Rows[0]["_AuthPersonName"].ToString(),
                                AuthPersonAADHAR = dt.Rows[0]["_AuthPersonAADHAR"] is DBNull ? string.Empty : dt.Rows[0]["_AuthPersonAADHAR"].ToString(),
                                CurrentAccountNo = dt.Rows[0]["_CurrentAccountNo"] is DBNull ? string.Empty : dt.Rows[0]["_CurrentAccountNo"].ToString(),
                                Address = dt.Rows[0]["_Address"] is DBNull ? string.Empty : dt.Rows[0]["_Address"].ToString(),
                                Block = dt.Rows[0]["_Block"] is DBNull ? string.Empty : dt.Rows[0]["_Block"].ToString(),
                                City = dt.Rows[0]["CityName"] is DBNull ? string.Empty : dt.Rows[0]["CityName"].ToString(),
                                District = dt.Rows[0]["Districtname"] is DBNull ? string.Empty : dt.Rows[0]["Districtname"].ToString(),
                                State = dt.Rows[0]["Statename"] is DBNull ? string.Empty : dt.Rows[0]["Statename"].ToString(),
                                Pincode = dt.Rows[0]["_Pincode"] is DBNull ? string.Empty : dt.Rows[0]["_Pincode"].ToString(),
                                IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                                Status = dt.Rows[0]["_Status"] is DBNull ? 1 : Convert.ToInt32(dt.Rows[0]["_Status"])
                            };
                        }
                    }



                }
            }
            catch (Exception ex) { }

            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetPartnerList";
    }
}
