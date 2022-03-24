using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.TronCoin
{
    public class TronGenerateAddressResp
    {
        public string privateKey { get; set; }
        public string publicKey { get; set; }
        public TronAddress address { get; set; }
    }
    public class TronAddress
    {
        public string base58 { get; set; }
        public string hex { get; set; }
    }
    public class TronGetBalanceResp
    {
        public bool success { get; set; }
        public List<TronGetBalanceData> data { get; set; }
    }
    public class TronGetBalanceData
    {
        public long balance { get; set; }
        public string address { get; set; }
        public List<TRC20> trc20 { get; set; }
    }
    public class TRC20
    {
        public string TLPCLksQPk5EbtKRfuTr2Dcf9sXwpmAzGu { get; set; }
    }
    public class TronINRResponse
    {
        [JsonProperty("tron-bsc")]
        public TronBSCData TronBSC { get; set; }
    }
    public class TronBSCData
    {
        public decimal inr { get; set; }
    }
    public class EasyTransferRequest
    {
        public string privateKey { get; set; }
        public string toAddress { get; set; }
        public int amount { get; set; }
    }
    public class EasyTransferRawRequest
    {
        public string privateKey { get; set; }
        public string senderAddress { get; set; }
        public string receiverAddress { get; set; }
        public string contractAddress { get; set; }
        public long amountInSun { get; set; }
    }
    public class TronFreezeRequest
    {
        public string privateKey { get; set; }
        public string senderAddress { get; set; }
        public string receiverAddress { get; set; }
        public string freezeFor { get; set; }
        public long amountInTRX { get; set; }
    }
    public class EasyTransferResponse
    {
        public bool result { get; set; }
        public string txid { get; set; }
        public TronTransaction transaction { get; set; }
    }
    public class TronTransaction
    {
        public bool visible { get; set; }
        public TronRawData raw_data { get; set; }
        public string raw_data_hex { get; set; }
        public List<string> signature { get; set; }
    }
    public class TronRawData
    {
        public List<TronContract> contract { get; set; }
        public string ref_block_bytes { get; set; }
        public string ref_block_hash { get; set; }
        public long expiration { get; set; }
        public long fee_limit { get; set; }
        public long timestamp { get; set; }
    }
    public class TronContract
    {
        public TronParameter parameter { get; set; }
    }
    public class TronParameter
    {
        public TronParamValue value { get; set; }
    }
    public class TronParamValue {
        public int frozen_duration { get; set; }
        public long frozen_balance { get; set; }
    }
}
