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
    public class ProcGetMobileTollFree : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetMobileTollFree(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }
        public object Call()
        {
            var res = new List<OperatorDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                 
                        foreach (DataRow row in dt.Rows)
                        {
                            var operatorDetail = new OperatorDetail
                            {
                                OID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                                TollFree = row["_TollFree"] is DBNull ? "" : row["_TollFree"].ToString()
                            };
                            res.Add(operatorDetail);
                        }
                  
                }
            }
            catch (Exception)
            {
            }
            return res;
        }
        public string GetName()
        {
            return "proc_GetMobileTollFree";
        }
       
    }

}
