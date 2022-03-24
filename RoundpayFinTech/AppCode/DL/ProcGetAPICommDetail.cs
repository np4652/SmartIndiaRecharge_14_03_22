using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPICommDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPICommDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID", req.CommonInt), 
                new SqlParameter("@LoginID", req.LoginID)
            };
            var _resp = new List<SlabCommission>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var slabDetail = new SlabCommission();
                    {
                        slabDetail.OID = Convert.ToInt32(dt.Rows[i]["OID"]);
                        slabDetail.Operator = dt.Rows[i]["Operator"].ToString();
                        slabDetail.IsBBPS = Convert.ToBoolean(dt.Rows[i]["IsBBPS"]);
                        slabDetail.OpType = Convert.ToInt32(dt.Rows[i]["OpType"]);
                        slabDetail.OperatorType = dt.Rows[i]["OperatorType"].ToString();
                        slabDetail.SlabID = Convert.ToInt32(dt.Rows[i]["APIID"]);
                        slabDetail.SlabDetailID = Convert.ToInt32(dt.Rows[i]["APIComID"] is DBNull ? "0" : dt.Rows[i]["APIComID"]);
                        slabDetail.Comm = Convert.ToDecimal(dt.Rows[i]["Commission"] is DBNull ? "0" : dt.Rows[i]["Commission"]);
                        slabDetail.CommType = Convert.ToInt32(dt.Rows[i]["CommType"] is DBNull ? "0" : dt.Rows[i]["CommType"]);
                        slabDetail.AmtType = Convert.ToInt32(dt.Rows[i]["AmtType"] is DBNull ? "0" : dt.Rows[i]["AmtType"]);
                        slabDetail.ModifyDate = dt.Rows[i]["ModifyDate"].ToString();
                    }
                    _resp.Add(slabDetail);
                }
            }
            catch{}
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAPICommDetail";
    }
}
