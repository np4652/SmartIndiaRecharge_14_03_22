using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDownOperators : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetDownOperators(IDAL dal) {
            _dal = dal;
        }
        public Task<object> Call(object obj)
        {
            throw new NotImplementedException();
        }

        public async Task<object> Call()
        {
            List<string> list = new List<string>();
            try
            {
                DataTable dt = await _dal.GetAsync(GetName());
                if (dt.Rows.Count > 0) {
                    foreach (DataRow item in dt.Rows)
                    {
                        list.Add(item["_Name"].ToString());
                    }
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        public string GetName()
        {
            return "select _Name From tbl_Operator where _IsActive=0 order by _Name";
        }
    }
}
