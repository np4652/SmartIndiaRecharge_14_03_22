using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public enum OrderStatus
    {
        OrderPlaced=1,
        Approved=2,
        Disapproved=3,
        Cancel=4,
        DeliveryAssigned = 5,
        Dispatched=6,
        Delivered=7,
        Closed=8,
    }
}
