using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Fields;
using PicasaDatabaseReader.Core.Tests;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.IntegrationTests
{
    public class DatabaseReaderIntegrationTests : TestsBase<DatabaseReaderIntegrationTests>
    {

        public DatabaseReaderIntegrationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected internal const string PathToDatabase = "C:\\Users\\Spade\\AppData\\Local\\Google\\Picasa2\\db3";

        [Fact]
        public async Task ShouldGetTableNames()
        {
            Logger.LogInformation("ShouldGetTableNames");

            var fileSystem = new FileSystem();

            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            var tableNames = await databaseReader
                .GetTableNames()
                .ToArray()
                .FirstAsync();

            tableNames.Should().HaveCount(3);
        }

        [Theory]
        [InlineData("albumdata", 20)]
        [InlineData("catdata", 3)]
        [InlineData("imagedata", 36)]
        public async Task ShouldGetFieldFiles(string tableName, int fieldCount)
        {
            Logger.LogInformation("ShouldGetFieldFiles TableName:{TableName} FieldCount:{FieldCount}", tableName, fieldCount);

            var fileSystem = new FileSystem();
            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            var fields = await databaseReader
                .GetFieldFilePaths(tableName)
                .ToArray()
                .FirstAsync();

            fields.Should().HaveCount(fieldCount);
        }

        [Fact]
        public async Task GetAlbumData()
        {
            Logger.LogInformation("GetAlbumData");
            var tableName = "albumdata";
            var result = await GetFields(tableName);

            IField[] core = result.core;

            PicasaDatabaseReader.Fields.IField[] legacy = result.legacy;

            var actual = core
                .Select(field => new
                {
                    name = field.Name,
                    type = field.Type,
                    count = field.Count
                })
                .OrderBy(arg => arg.name)
                .ToArray();

            var expected = legacy
                .Select(field => new
                {
                    name = field.Name,
                    type = field.Type,
                    count = field.Count
                })
                .OrderBy(arg => arg.name)
                .ToArray();

            actual
                .Should()
                .BeEquivalentTo(expected);
        }

        private async Task<(IField[] core, PicasaDatabaseReader.Fields.IField[] legacy)> GetFields(string tableName)
        {
            var core = await GetCoreFields(tableName).ToArray().FirstAsync();
            var legacy = await LegacyGetFields(tableName).ToArray().FirstAsync();

            return (core: core, legacy: legacy);
        }

        private IObservable<IField> GetCoreFields(string tableName)
        {
            var fileSystem = new FileSystem();
            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            return databaseReader
                .GetFields(tableName);
        }

        private IObservable<PicasaDatabaseReader.Fields.IField> LegacyGetFields(string tableName)
        {
            var fileSystem = new FileSystem();
            var databaseReader = this.CreateDatabaseReader(fileSystem, PathToDatabase);

            return databaseReader
                .GetFieldFilePaths(tableName)
                .Select(PicasaDatabaseReader.FieldFactory.CreateField);
        }
    }
}
