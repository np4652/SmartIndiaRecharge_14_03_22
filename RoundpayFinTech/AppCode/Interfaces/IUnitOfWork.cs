using Fintech.AppCode.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IUnitOfWork
    {
        IReportML reportML { get; }
        IUserML userML { get; }
        ILoginML loginML { get; }
        IAdminML adminML { get; }
        IAppReportML appReportML { get; }
        ISlabML slabML { get; } 
        IWhatsappML whatsappML { get; }
    }
}
