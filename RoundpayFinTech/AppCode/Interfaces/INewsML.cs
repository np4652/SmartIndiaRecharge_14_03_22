using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface INewsML
    {
        News AddNews();
        UserNews GetNews();
        News EditNews(int Id);
        IResponseStatus CallAddNews(News _req);
        IResponseStatus DeleteNews(CommonReq newsReq);
        UserNews GetNewsRoleAssign(int Id);
        List<News> GetNewsByRole(int Id);
    }
}
