using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface INewsAppML
    {
        News GetNewsRoleNewsForApp(CommonReq commonReq);
    }

    public interface IUpdateNewsML
    {
        //UserNews UpdateNews(News newsReq);
        IResponseStatus UpdateNews(News newsReq);

    }
    public interface IDeleteNewsML
    {
        IResponseStatus DeleteNews(News newsReq);
    }
}
