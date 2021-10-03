using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rescheduler.Infra
{
    public static class ILoggerExtensions
    {
        public static LoggerTimer Time(this ILogger logger, LogLevel logLevel, string msgTemplate, params object[] msgParams)
        {
            return new LoggerTimer(logger, logLevel, msgTemplate, msgParams);
        }
    }

    public class LoggerTimer : IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;
        private readonly LogLevel _logLevel;
        private readonly string _msgTemplate;
        private readonly object[] _msgParams;
        private readonly Stopwatch _sw;
        
        public LoggerTimer(ILogger logger, LogLevel logLevel, string msgTemplate, object[] msgParams)
        {
            _logger = logger;
            _logLevel = logLevel;
            _msgTemplate = string.Concat(msgTemplate, " completed in {Duration}");
            _msgParams = msgParams;
            
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _sw.Stop();
            var args = _msgParams.Append(_sw.Elapsed);
            _logger.Log(_logLevel, _msgTemplate, args);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}