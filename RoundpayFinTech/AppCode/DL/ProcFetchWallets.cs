using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFetchWallets : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFetchWallets(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();

        public object Call()
        {
            var res = new List<WalletType>();
            var dt = _dal.Get(GetName());
            if (dt.Rows.Count > 0) {
                foreach (DataRow item in dt.Rows)
                {
                    res.Add(new WalletType {
                        ID=item["_ID"] is DBNull?0:Convert.ToInt16(item["_ID"]),
                        Name = item["_WalletType"] is DBNull ? "" : item["_WalletType"].ToString(),
                        IsActive = item["_Status"] is DBNull ? false : Convert.ToBoolean(item["_Status"]),
                        InFundProcess = item["_InFundProcess"] is DBNull ? false : Convert.ToBoolean(item["_InFundProcess"]),
                        IsPackageDedectionForRetailor = item["_IsPackageDedectionForRetailor"] is DBNull ? false : Convert.ToBoolean(item["_IsPackageDedectionForRetailor"])
                    });
                }
            }
            return res;
        }

        public string GetName()
        {
            return "select _ID,_WalletType,_Status,_InFundProcess,_IsPackageDedectionForRetailor from MASTER_WALLET_TYPE where _Status=1 order by _ID";
        }
    }
}
