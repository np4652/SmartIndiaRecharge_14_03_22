using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetExactness : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetExactness(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();

        public object Call()
        {
            var res = new List<EXACTNESSMaster>();
            var dt = _dal.Get(GetName());
            if (dt.Rows.Count > 0) {
                foreach (DataRow item in dt.Rows)
                {
                    res.Add(new EXACTNESSMaster
                    {
                        ID = item["_ID"] is DBNull ? 0 : Convert.ToInt16(item["_ID"]),
                        EXACTNESS = item["_ExactNess"] is DBNull ? "" : item["_ExactNess"].ToString(),
                    });
                }
            }
            return res;
        }

        public string GetName()
        {
            return "Select * from MASTER_EXACTNESS order by _ID";
        }
    }
}
