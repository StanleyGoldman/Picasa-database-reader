using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PicasaDatabaseReader.Core.Scheduling;
using Serilog;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public static class TestExtensions
    {
        public static DatabaseReader CreateDatabaseReader<T>(this TestsBase<T> databaseReaderTests,
            IFileSystem fileSystem,
            string directoryPath,
            ISchedulerProvider testScheduleProvider = null)
        {
            var serviceCollection = databaseReaderTests.GetServiceCollection()
                .AddSingleton(fileSystem)
                .AddScoped(provider => new DatabaseReader(provider.GetService<IFileSystem>(), directoryPath,
                    provider.GetService<ILogger>(), provider.GetService<ISchedulerProvider>()));

            if (testScheduleProvider != null)
            {
                serviceCollection.AddSingleton(testScheduleProvider);
            }

            var buildServiceProvider = serviceCollection
                .BuildServiceProvider();

            return buildServiceProvider.GetService<DatabaseReader>();
        }
    }
}