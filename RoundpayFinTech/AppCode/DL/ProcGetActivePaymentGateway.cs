using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetActivePaymentGateway : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetActivePaymentGateway(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }
        public object Call()
        {
            var res = new List<MasterPGateway>();
            
            try
            {
                DataTable dt = _dal.Get("Select mp._ID, mp._Name from tbl_PaymentGateway pg inner join MASTER_PGATEWAY mp on mp._ID=pg._PGID where pg._IsActive= 1 order by 1");
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var operatorDetail = new MasterPGateway
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            PGName = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            
                        };
                        res.Add(operatorDetail);
                    }

                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
        public string GetName()
        {
            return "Select mp._ID, mp._Name from tbl_PaymentGateway pg inner join MASTER_PGATEWAY mp on mp._ID=pg._PGID where pg._IsActive= 1 order by 1";
        }

    }

}
