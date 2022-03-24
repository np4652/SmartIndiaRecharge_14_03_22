using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Coin
{
    public class CoinCreditServiceProcRequest
    {
        public int UserID { get; set; }
        public string SenderAddrss { get; set; }
        public string ReceiverAddress { get; set; }
        public long AmountInCoin { get; set; }
        public string AmountType { get; set; }
        public decimal AmountInINR { get; set; }
        public int OID { get; set; }
    }
    public class CoinCreditServiceUpdateProcReq
    {
        public int TID { get; set; }
        public int Status { get; set; }
        public string VendorID { get; set; }
        public string LiveID { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
    
    public class CoinCreditServiceProcResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
    }
    public class CoinAddressDetail
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string CoinToken { get; set; }
        public string CoinAddress { get; set; }
        public string CoinHexAddress { get; set; }
        public long CoinLastBalance { get; set; }
        public long CoinSmartContractBalance { get; set; }
        public int OID { get; set; }
        public bool IsFrozen { get; set; }
        public string CoinReceiverAddress { get; set; }
        public string SPKey { get; set; }
    }
    public class MoneyConversionRate
    {
        public string FromSymbol { get; set; }
        public int FromSymbolID { get; set; }
        public int ToSymbolID { get; set; }
        public string ToSymbol { get; set; }
        public int Ind { get; set; }
        public decimal Rate { get; set; }
    }
    public class RateWithStatus
    {
        public bool IsFound { get; set; }
        public decimal Rate { get; set; }
    }
    public class CoinBalanceRequest
    {
        public string CoinAddress { get; set; }
    }
    public class CoinBalanceResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public long Balance { get; set; }
        public long SmartContractBalance { get; set; }
    }
    public class CoinEasyTransferReq
    {
        public string Token { get; set; }
        public string Address { get; set; }
        public string SenderAddress { get; set; }
        public string FreezeFor { get; set; }
        public long Amount { get; set; }
    }

    public class CoinTransferResponse
    {
        public int Statuscode { get; set; }
        public int Status { get; set; }
        public int FreezeDuration { get; set; }
        public string Msg { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
    }
}
