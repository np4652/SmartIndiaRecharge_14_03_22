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
    public class ProcAdminSettlement : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAdminSettlement(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new List<AdminSettlement>();
            SqlParameter[] param = {
                new SqlParameter("@FromDate",req.CommonStr??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate",req.CommonStr2??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@WalletTypeID",req.CommonInt)
            };
            var dt = _dal.Get(GetName(),param);
            if (dt.Rows.Count > 0) {
                foreach (DataRow item in dt.Rows)
                {
                    res.Add(new AdminSettlement {
                        _TransactionDate=item["_TransactionDate"] is DBNull?"":Convert.ToDateTime(item["_TransactionDate"]).ToString("dd MMM yyyy"),
                        _Opening = item["_Opening"] is DBNull ? 0 : Convert.ToDouble(item["_Opening"]),
                        _FundTransfered = item["_FundTransfered"] is DBNull ? 0 : Convert.ToDouble(item["_FundTransfered"]),
                        _Refund = item["_Refund"] is DBNull ? 0 : Convert.ToDouble(item["_Refund"]),
                        _Commission = item["_Commission"] is DBNull ? 0 : Convert.ToDouble(item["_Commission"]),
                        _CCFComm = item["_CCFComm"] is DBNull ? 0 : Convert.ToDouble(item["_CCFComm"]),
                        _FundDeducted = item["_FundDeducted"] is DBNull ? 0 : Convert.ToDouble(item["_FundDeducted"]),
                        _Surcharge = item["_Surcharge"] is DBNull ? 0 : Convert.ToDouble(item["_Surcharge"]),
                        _SuccessPrepaid = item["_SuccessPrepaid"] is DBNull ? 0 : Convert.ToDouble(item["_SuccessPrepaid"]),
                        _TotalSUccess = item["_TotalSUccess"] is DBNull ? 0 : Convert.ToDouble(item["_TotalSUccess"]),
                        _SuccessPostpaid = item["_SuccessPostpaid"] is DBNull ? 0 : Convert.ToDouble(item["_SuccessPostpaid"]),
                        _SuccessDTH = item["_SuccessDTH"] is DBNull ? 0 : Convert.ToDouble(item["_SuccessDTH"]),
                        _SuccessBill = item["_SuccessBill"] is DBNull ? 0 : Convert.ToDouble(item["_SuccessBill"]),
                        _SuccessDMT = item["_SuccessDMT"] is DBNull ? 0 : Convert.ToDouble(item["_SuccessDMT"]),
                        _SuccessDMTCharge = item["_SuccessDMTCharge"] is DBNull ? 0 : Convert.ToDouble(item["_SuccessDMTCharge"]),
                        _OtherCharge = item["_OtherCharge"] is DBNull ? 0 : Convert.ToDouble(item["_OtherCharge"]),
                        _CCFCommDebited = item["_CCFCommDebited"] is DBNull ? 0 : Convert.ToDouble(item["_CCFCommDebited"]),
                        _EntryDate = item["_EntryDate"] is DBNull ? "" : Convert.ToDateTime(item["_EntryDate"]).ToString("dd MMM yyyy"),
                        _Closing = item["_Closing"] is DBNull ? 0 : Convert.ToDouble(item["_Closing"]),
                        _Expected = item["_Expected"] is DBNull ? 0 : Convert.ToDouble(item["_Expected"])
                    });
                }
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "select _TransactionDate,_Opening,_FundTransfered,_Refund,_Commission,_CCFComm,_FundDeducted,_Surcharge,_TotalSUccess,_SuccessPrepaid,_SuccessPostpaid,_SuccessDTH,_SuccessBill,_SuccessDMT,_SuccessDMTCharge,_OtherCharge,_CCFCommDebited,_EntryDate,_Closing,(_Opening+_FundTransfered+_Refund+_Commission+_CCFComm)-(_Surcharge+_FundDeducted+_OtherCharge+_TotalSuccess+_CCFCommDebited) _Expected From tbl_AdminSettlement where _TransactionDate>=cast(@FromDate as date) and _TransactionDate<=cast(@ToDate as date) and _WalletID=@WalletTypeID order by _TransactionDate";
        }
    }
}
