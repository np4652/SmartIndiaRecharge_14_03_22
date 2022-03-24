using System.Threading.Tasks;

namespace Fintech.AppCode.Interfaces
{
    public interface IProcedure
    {
        string GetName();
        object Call(object obj);
        object Call();
    }
    public interface IProcedureAsync
    {
        Task<object> Call(object obj);
        Task<object> Call();
        string GetName();
    }
}
