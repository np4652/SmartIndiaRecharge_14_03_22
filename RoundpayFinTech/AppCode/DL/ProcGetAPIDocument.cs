using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPIDocument : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPIDocument(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            var model = new GetApiDocument();
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.CommonInt),
                new SqlParameter("@LT", _req.CommonInt2),
                new SqlParameter("@WID", _req.CommonInt3)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    
                    model.Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
                    model.EmailTechnical = dt.Rows[0]["_EmailTechnical"] is DBNull ? "" : dt.Rows[0]["_EmailTechnical"].ToString();
                    model.MobileTechnical = dt.Rows[0]["_MobileTechnical"] is DBNull ? "" : dt.Rows[0]["_MobileTechnical"].ToString();
                    model.EmployeeID = dt.Rows[0]["_EmpID"] is DBNull ? "" : dt.Rows[0]["_EmpID"].ToString();
                    model.UserMobile = dt.Rows[0]["_UserMobile"] is DBNull ? "" : dt.Rows[0]["_UserMobile"].ToString();
                    model.UserID = _req.CommonInt;
                    model.SubDomain = dt.Rows[0]["_SubDomain"] is DBNull ? "" : dt.Rows[0]["_SubDomain"].ToString();
                    model.StatusCheckDoamin = dt.Rows[0]["_StatusCheckDoamin"] is DBNull ? "" : dt.Rows[0]["_StatusCheckDoamin"].ToString();
                }
            }
            catch (Exception er)
            {

            }
            return model;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAPIDocumentValues";
    }
}
