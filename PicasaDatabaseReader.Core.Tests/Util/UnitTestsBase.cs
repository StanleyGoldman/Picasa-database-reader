using Bogus;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public abstract class UnitTestsBase : TestsBase
    {
        protected UnitTestsBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}