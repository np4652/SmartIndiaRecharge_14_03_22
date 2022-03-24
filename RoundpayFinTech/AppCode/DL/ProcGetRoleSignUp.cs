using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetRoleSignUp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRoleSignUp(IDAL dal) => _dal = dal;
        public object Call()
        {
            var Roles = new List<RoleMaster>();
            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                            RoleMaster roleMaster = new RoleMaster
                            {
                                ID = Convert.ToInt32(dr["_ID"]),
                                Role = dr["_Role"].ToString(),
                            };
                            Roles.Add(roleMaster);
                    }
                }
            }
            catch (Exception ex) { }
            return Roles;
        }

        public object Call(object obj)
        {
            throw new NotImplementedException();
        }
        public string GetName() => "Proc_GetRole_SignUp";
    }



}
