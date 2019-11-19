using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutofacSerilogIntegration;
using FluentAssertions;
using PicasaDatabaseReader.Core.Scheduling;
using PicasaDatabaseReader.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace PicasaDatabaseReader.Core.Tests
{
    public class DatabaseReaderTests : UnitTestsBase
    {
        protected internal readonly TestScheduleProvider TestScheduleProvider = new TestScheduleProvider();
        private readonly MockFileSystem _mockFileSystem;
        private readonly IContainer _container;

        public DatabaseReaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _mockFileSystem = new MockFileSystem();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterLogger();
            containerBuilder.RegisterInstance(_mockFileSystem).As<IFileSystem>();
            containerBuilder.RegisterInstance(TestScheduleProvider).As<IUISchedulerProvider>();
            containerBuilder.RegisterType<DatabaseReader>();

            _container = containerBuilder.Build();
        }

        [Fact]
        public void ShouldGetTableNames()
        {
            Logger.Information("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"{name}_0";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(BitConverter.GetBytes(DatabaseReader.TableFileHeader));
            foreach (var arg in args)
            {
                _mockFileSystem.AddFile(arg.path, mockFileData);
            }

            var databaseReader = _container.Resolve<DatabaseReader>();
            databaseReader.Initialize(directoryPath);

            var tableNames = databaseReader
                .GetTableNames();

            var autoResetEvent = new AutoResetEvent(false);

            tableNames.Subscribe(_ => { }, () => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        [Fact]
        public async Task ShouldGetFieldFiles()
        {
            Logger.Information("ShouldGetFieldFiles");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var args = Faker.Lorem.Words()
                .Distinct()
                .Select(name =>
                {
                    var filename = $"TestTable_{name}.pmp";
                    var path = Path.Combine(directoryPath, filename);
                    return new { name, filename, path };
                })
                .ToArray();

            var mockFileData = new MockFileData(string.Empty);
            foreach (var arg in args)
            {
                _mockFileSystem.AddFile(arg.path, mockFileData);
            }

            var databaseReader = _container.Resolve<DatabaseReader>();
            databaseReader.Initialize(directoryPath);

            var tableNames = await databaseReader
                .GetFieldFilePaths("TestTable")
                .ToArray()
                .FirstAsync();

            tableNames.Should().BeEquivalentTo(args.Select(arg => arg.path));
        }

        [Fact]
        public void ShouldNotGetThumbIndex()
        {
            Logger.Information("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var databaseReader = _container.Resolve<DatabaseReader>();
            databaseReader.Initialize(directoryPath);

            var thumbIndex = databaseReader.GetThumbIndex();

            var autoResetEvent = new AutoResetEvent(false);

            thumbIndex.Subscribe(_ => { }, (ex) => autoResetEvent.Set());

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();
        }

        [Fact]
        public void ShouldGetThumbIndex()
        {
            Logger.Information("ShouldGetTableNames");

            var directoryPath = Path.Combine("c:\\", string.Join("\\", Faker.Lorem.Words()));

            var fileCount = Faker.Random.Int(4, 10);

            var thumbIndexContent = new List<byte> { 0x66, 0x66, 0x46, 0x40 };
            thumbIndexContent.AddRange(BitConverter.GetBytes(fileCount));

            var fileInputs = Enumerable.Repeat(Unit.Default, fileCount)
                .Select(_ => (index: Faker.Random.UInt(0, 100), word: Faker.Random.Word()))
                .ToArray();

            foreach (var fileInput in fileInputs)
            {
                thumbIndexContent.AddRange(Encoding.ASCII.GetBytes(fileInput.word));
                thumbIndexContent.Add(0x00);
                thumbIndexContent.AddRange(Enumerable.Repeat<byte>(0x00, 26));
                thumbIndexContent.AddRange(BitConverter.GetBytes(fileInput.index));
            }

            _mockFileSystem.AddFile(Path.Combine(directoryPath, "thumbindex.db"), new MockFileData(thumbIndexContent.ToArray()));
            
            var databaseReader = _container.Resolve<DatabaseReader>();
            databaseReader.Initialize(directoryPath);

            var thumbIndex = databaseReader.GetThumbIndex().ToArray();

            var autoResetEvent = new AutoResetEvent(false);

            IndexData[] indexData = null;
            thumbIndex.Subscribe(Observer.Create<IndexData[]>(
                onNext: datas =>
                {
                    indexData = datas;
                },
                onError: exception =>
                {
                },
                onCompleted: () =>
                {
                    autoResetEvent.Set();
                }));

            TestScheduleProvider.ThreadPool.AdvanceBy(1);

            autoResetEvent.WaitOne();

            indexData.Should().NotBeNull();
            indexData.Should().HaveCount(fileCount);

            for (int i = 0; i < indexData.Length; i++)
            {
                indexData[i].Content.Should().Be(fileInputs[i].word);
                indexData[i].Index.Should().Be(fileInputs[i].index);
            }
        }
    }
}
