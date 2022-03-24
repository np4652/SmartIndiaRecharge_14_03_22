using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSlabChange : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSlabChange(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param =
                {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@UserID",req.CommonInt)
            };
            GetChangeSlab _res = new GetChangeSlab()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Columns.Contains("Msg") ? dt.Rows[0]["Msg"].ToString() : "";
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.Msg = ErrorCodes.SUCCESS;
                        _res.CommonStr = dt.Rows[0]["OutletName"].ToString();
                        _res.CommonStr2 = dt.Rows[0]["MobileNo"].ToString();
                        List<SlabMaster> slabMasters = new List<SlabMaster>();
                        foreach (DataRow dr in dt.Rows)
                        {
                            SlabMaster slabMaster = new SlabMaster();
                            slabMaster.ID = Convert.ToInt32(dr["_ID"]);
                            slabMaster.Slab = dr["_Slab"].ToString();
                            slabMaster.IsAdminDefined = dt.Rows[0]["_IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAdminDefined"]);
                            slabMasters.Add(slabMaster);
                        }
                        _res.Slabs=slabMasters;
                    }
                }

            }
            catch (SystemException ex) { }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetSlabChange";
        }
    }
}
