using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface ICardMapping
    {
        IEnumerable<CardAccountMapping> GetCardAccount(int UserId);
        IResponseStatus UpdateCardAccount(CardAccountMapping req);
        IResponseStatus DeleteCardAccount(CardAccountMapping req);
    }
}
