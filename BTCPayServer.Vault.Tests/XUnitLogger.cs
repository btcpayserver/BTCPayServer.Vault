using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BTCPayServer.Vault.Tests
{
    class XUnitLoggerFactory : ILoggerFactory
    {
        public XUnitLoggerFactory(ITestOutputHelper testOutput)
        {
            TestOutput = testOutput;
        }

        public ITestOutputHelper TestOutput { get; }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitLogger(categoryName, TestOutput);
        }

        public void Dispose()
        {
            
        }
    }
    class XUnitLogger : ILogger
    {
        class NullDisposable : IDisposable
        {
            public void Dispose()
            {
                
            }
        }

        private readonly string _prefix;
        private ITestOutputHelper _testOutput;

        public XUnitLogger(string prefix, ITestOutputHelper testOutput)
        {
            _prefix = prefix;
            _testOutput = testOutput;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NullDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _testOutput.WriteLine($"{_prefix}: {formatter(state, exception)}");
            if (exception != null)
                _testOutput.WriteLine(exception.ToString());
        }
    }
}
