using Vending.EntityLayer.Concrete;

namespace Vending.BusinessLayer.Abstract
{
    public interface ILogService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception);
        IEnumerable<Log> GetAllLogs(); // Add method to retrieve all logs
    }
}
