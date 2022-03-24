using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetShopppingSlabComm : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetShopppingSlabComm(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ShoppingCommissionReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@SlabID",req.SlabID),
                new SqlParameter("@CommType",ApplicationSetting.ECommerceDistributionCommissionType),
            };
            var response = new ShoppingCommissionExtend();            
            var res = new List<ShoppingCommission>();
            var items = new List<ShoppingCommission>();
            var role = new List<RoleMaster>();
            try
            {
                if (!req.IsAdminDefined)
                {
                    DataTable dt = _dal.GetByProcedure(GetName(), param);
                    if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) != -1)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            res.Add(new ShoppingCommission
                            {
                                CategoryID = Convert.ToInt32(dr["_CategoryID"]),
                                CategoryName = Convert.ToString(dr["_CategoryName"]),
                                Commission = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]),
                                AmountType = dr["_commType"] is DBNull ? false : Convert.ToBoolean(dr["_commType"])
                            });
                        }
                    }
                }
                else
                {
                    DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) != -1)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            res.Add(new ShoppingCommission
                            {
                                CategoryID = Convert.ToInt32(dr["_CategoryID"]),
                                CategoryName = Convert.ToString(dr["_CategoryName"]),
                                Commission = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]),
                                RoleID = dr["_RoleID"] is DBNull ? 0 : Convert.ToInt32(dr["_RoleID"]),
                                AmountType = dr["_commType"] is DBNull ? false : Convert.ToBoolean(dr["_commType"])
                            });
                        }
                    }

                    if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) != -1)
                    {
                        dt = ds.Tables[1];
                        if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) != -1)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                items.Add(new ShoppingCommission
                                {
                                    CategoryID = Convert.ToInt32(dr["_CategoryID"]),
                                    CategoryName = Convert.ToString(dr["_CategoryName"])
                                });
                            }
                        }
                    }

                    if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) != -1)
                    {
                        dt = ds.Tables[2];
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                var data = new RoleMaster
                                {
                                    ID = Convert.ToInt32(dr["_ID"]),
                                    Role = Convert.ToString(dr["_Role"])
                                };
                                role.Add(data);
                            }
                        }
                    }
                    response.Roles = role;
                }
                response.CommissionDetail = res;
                response.Items = items;
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_getShopppingSlabComm";
    }
}