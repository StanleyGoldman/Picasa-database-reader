using Bogus;
using PicasaDatabaseReader.Core.Tests.Logging;
using Serilog;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public abstract class TestsBase
    {
        protected readonly Faker Faker;

        protected readonly ILogger Logger;

        protected TestsBase(ITestOutputHelper testOutputHelper)
        {
            Faker = new Faker();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithThreadId()
                .Enrich.With<CustomEnrichers>()
                .WriteTo.TestOutput(testOutputHelper,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] ({PaddedThreadId}) {ShortSourceContext} {Message}{NewLineIfException}{Exception}")
                .CreateLogger();

            Logger = Log.Logger.ForContext(GetType());
        }
    }
}