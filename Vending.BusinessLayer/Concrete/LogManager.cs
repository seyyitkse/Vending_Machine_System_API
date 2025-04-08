using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.EntityLayer.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace Vending.BusinessLayer.Concrete
{
    public class LogManager : ILogService
    {
        private readonly VendingContext _context;

        public LogManager(VendingContext context)
        {
            _context = context;
        }

        public void LogInformation(string message)
        {
            Log log = new Log
            {
                Timestamp = DateTime.Now,
                LogLevel = "Information",
                Message = message,
                Exception = string.Empty // Set Exception to an empty string
            };
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public void LogWarning(string message)
        {
            Log log = new Log
            {
                Timestamp = DateTime.Now,
                LogLevel = "Warning",
                Message = message,
                Exception = string.Empty // Set Exception to an empty string
            };
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public void LogError(string message, Exception exception)
        {
            Log log = new Log
            {
                Timestamp = DateTime.Now,
                LogLevel = "Error",
                Message = message,
                Exception = exception.ToString()
            };
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public IEnumerable<Log> GetAllLogs()
        {
            return _context.Logs.ToList(); // Retrieve all logs from the database
        }
    }
}
