using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetState : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetState(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }
        public object Call()
        {
            List<StateMaster> states = new List<StateMaster>();
            
            DataTable dt = _dal.Get(GetName());
            foreach (DataRow dr in dt.Rows)
            {
                StateMaster state = new StateMaster() {
                    StateID = Convert.ToInt32(dr["_ID"]),
                    StateName = dr["StateName"].ToString()
                };
                states.Add(state);
            }
            return states;
        }
        public string GetName()
        {
            return "select * from master_state  order by _ID";
        }
    }
}
