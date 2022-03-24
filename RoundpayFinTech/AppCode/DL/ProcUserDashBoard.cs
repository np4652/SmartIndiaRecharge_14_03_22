using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Fintech.AppCode.DL
{
    public class ProcUserDashBoard : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserDashBoard(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            var _res = new List<PieChartList>();

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                foreach (DataRow item in dt.Rows)
                {
                    var _chart = new PieChartList
                    {
                        Service = item["_OpType"].ToString(),
                        Amount = Convert.ToDecimal(item["SAmount"])
                    };
                    _res.Add(_chart);
                }
              
            }
            catch (Exception ex)
            {
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_UserDashboard";
        }
    }
}
