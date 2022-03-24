using RoundpayFinTech.AppCode.Model.Coin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ICoinML
    {
        CoinAddressDetail GenerateQR(string SPKey, int UserID,int OID=0);
        CoinAddressDetail CheckBalance(string SPKey, int UserID, int OID = 0);
        List<MoneyConversionRate> GetRates(int OID);
    }
}
