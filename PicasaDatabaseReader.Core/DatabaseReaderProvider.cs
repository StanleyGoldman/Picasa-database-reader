﻿using System;
using System.Data;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PicasaDatabaseReader.Core.Extensions;
using PicasaDatabaseReader.Core.Fields;
using PicasaDatabaseReader.Core.Interfaces;
using PicasaDatabaseReader.Core.Scheduling;
using Serilog;

namespace PicasaDatabaseReader.Core
{
    public class DatabaseReaderProvider : IDatabaseReaderProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _loggerFactory;
        private readonly ISchedulerProvider _scheduler;

        public DatabaseReaderProvider(IFileSystem fileSystem, ILogger loggerFactory, ISchedulerProvider scheduler = null)
        {
            _fileSystem = fileSystem;
            _loggerFactory = loggerFactory;
            _scheduler = scheduler;
        }

        public DatabaseReader GetDatabaseReader(string pathToDatabase)
        {
            return new DatabaseReader(_fileSystem, pathToDatabase, _loggerFactory.ForContext<DatabaseReader>(), _scheduler);
        }
    }
}
