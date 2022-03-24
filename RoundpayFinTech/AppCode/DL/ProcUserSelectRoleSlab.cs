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
    public class ProcUserSelectRoleSlab : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserSelectRoleSlab(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int UserID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", UserID)
            };
            var _res = new UserRoleSlab();
            var Roles = new List<RoleMaster>();
            var Slabs = new List<SlabMaster>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (Convert.ToInt32(dr["TypeId"]) == 1)
                        {
                            var roleMaster = new RoleMaster
                            {
                                ID = Convert.ToInt32(dr["ValueField"]),
                                Role = dr["TextField"].ToString(),
                                Ind = Convert.ToInt32(dr["Sno"])
                            };
                            Roles.Add(roleMaster);
                        }
                        else
                        {
                            var slabMaster = new SlabMaster
                            {
                                ID = Convert.ToInt32(dr["ValueField"]),
                                Slab = dr["TextField"].ToString()
                            };
                            Slabs.Add(slabMaster);
                        }
                    }
                    _res.Roles = Roles.OrderBy(x => x.Ind).ToList();
                    _res.Slabs = Slabs;
                }
            }
            catch (Exception ex){}
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_User_SelectRoleSlab";
    }


    public class SelectRole : IProcedure
    {
        private readonly IDAL _dal;
        public SelectRole(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int UserID = (int)obj;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@UserID", UserID);
            var Roles = new List<RoleMaster>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (Convert.ToInt32(dr["TypeId"]) == 1)
                        {
                            RoleMaster roleMaster = new RoleMaster
                            {
                                ID = Convert.ToInt32(dr["ValueField"]),
                                Role = dr["TextField"].ToString(),
                                Ind = Convert.ToInt32(dr["Sno"])
                            };
                            Roles.Add(roleMaster);
                        }

                    }
                }
            }
            catch (Exception ex){}
            return Roles;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_User_SelectRole";
    }

    public class FnGetChildRoles : IProcedure
    {
        private readonly IDAL _dal;
        public FnGetChildRoles(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int RoleID = (int)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@RoleID", RoleID)
            };
            var Roles = new List<RoleMaster>();
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var roleMaster = new RoleMaster
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            Role = dr["_Role"].ToString(),
                            Ind = Convert.ToInt32(dr["_Ind"])
                        };
                        Roles.Add(roleMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = RoleID,
                    UserId = RoleID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Roles;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "select * from dbo.fn_GetChildRoles(@RoleID) order by _Ind";
        }
    }


    public class FnGetChildRolesForNews : IProcedure
    {
        private readonly IDAL _dal;
        public FnGetChildRolesForNews(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@RoleID", req.CommonInt2),
                new SqlParameter("@NewsId", req.CommonInt)
            };
            var Roles = new List<RoleMaster>();
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var roleMaster = new RoleMaster
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            Role = dr["_Role"].ToString(),
                            Ind = Convert.ToInt32(dr["_Ind"]),
                            IsActive = Convert.ToBoolean(dr["_isEligible"]),
                        };
                        Roles.Add(roleMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.CommonInt,
                    UserId = req.CommonInt
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Roles;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "select r.*,Case(ISNULL(nd.RoleID,0)) when 0 then 0 else 1 end _isEligible from dbo.fn_GetChildRoles(@RoleID) r Left join tbl_NewsDetail nd on nd.RoleID=r._id and nd.NewsId=@NewsId  order by _Ind";
    }
}
