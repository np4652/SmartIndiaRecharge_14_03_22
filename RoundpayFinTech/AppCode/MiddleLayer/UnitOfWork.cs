using Fintech.AppCode;
using Fintech.AppCode.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Interfaces;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private IReportML report;
        private IUserML user;
        private ILoginML login;
        private IAdminML admin;
        private IAppReportML appReport;
        private ISlabML slab;
        private IWhatsappML whatsapp;

        public UnitOfWork(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
        public IReportML reportML
        {
            get
            {
                return report ?? (report = new ReportML(_accessor, _env));
            }
        }

        public IUserML userML
        {
            get
            {
                return user ?? (user = new UserML(_accessor, _env));
            }
        }

        public ILoginML loginML
        {
            get
            {
                return login ?? (login = new LoginML(_accessor, _env));
            }
        }

        public IAdminML adminML
        {
            get
            {
                return admin ?? (admin = new UserML(_accessor, _env));
            }
        }

        public IAppReportML appReportML
        {
            get
            {
                return appReport ?? (appReport = new ReportML(_accessor, _env));
            }
        }

        public ISlabML slabML
        {
            get
            {
                return slab ?? (slab = new SlabML(_accessor, _env));
            }
        }
        public IWhatsappML whatsappML
        {
            get
            {
                return whatsapp ?? (whatsapp = new WhatsappML(_accessor, _env));
            }
        }
    }
}
