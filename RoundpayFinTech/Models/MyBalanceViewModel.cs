using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class MyBalanceViewModel
    {
        public UserBalnace userBalnace { get; set; }
        public IEnumerable<OperatorDetail> transactionModes { get; set; }
        public List<MoveToWalletMapping> moveToWalletMappings { get; set; }
    }
}
