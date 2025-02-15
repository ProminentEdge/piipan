using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Match.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class MatchRecordDaoTests : DbFixture
    {
        private IDbConnectionFactory<CollaborationDb> DbConnFactory()
        {
            var factory = new Mock<IDbConnectionFactory<CollaborationDb>>();
            factory
                .Setup(m => m.Build(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    var conn = Factory.CreateConnection();
                    conn.ConnectionString = ConnectionString;
                    conn.Open();
                    return conn;
                });

            return factory.Object;
        }

        [Fact]
        public async Task AddRecord()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchRecordDao>>();
                var dao = new MatchRecordDao(DbConnFactory(), logger);
                var idService = new MatchIdService();
                var record = new MatchRecordDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Status = MatchRecordStatus.Open,
                    Invalid = false,
                    Data = "{}"
                };

                // Act
                await dao.AddRecord(record);

                // Assert
                Assert.True(HasRecord(record));
            }
        }

        [Fact]
        public async Task AddRecord_MatchIdCollision()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchRecordDao>>();
                var dao = new MatchRecordDao(DbConnFactory(), logger);
                var idService = new MatchIdService();
                var record = new MatchRecordDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Status = MatchRecordStatus.Open,
                    Invalid = false,
                    Data = "{}"
                };

                // Act
                await dao.AddRecord(record);

                // Assert (re-insert same record)
                await Assert.ThrowsAsync<DuplicateMatchIdException>(() => dao.AddRecord(record));
            }
        }

        // AddRecord() should let PostgresExceptions bubble up
        // if they are not unique constraint violations
        [Fact]
        public async Task AddRecord_PostgresException()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchRecordDao>>();
                var dao = new MatchRecordDao(DbConnFactory(), logger);
                var idService = new MatchIdService();

                // Invalid JSON format for Data property
                var record = new MatchRecordDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Status = MatchRecordStatus.Open,
                    Invalid = false,
                    Data = "{{"
                };

                // Act / Assert
                await Assert.ThrowsAsync<PostgresException>(() => dao.AddRecord(record));
            }
        }

        [Fact]
        public async Task AddRecord_ReturnsMatchId()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchRecordDao>>();
                var dao = new MatchRecordDao(DbConnFactory(), logger);
                var idService = new MatchIdService();
                var record = new MatchRecordDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Status = MatchRecordStatus.Open,
                    Invalid = false,
                    Data = "{}"
                };

                // Act
                string result = await dao.AddRecord(record);

                // Assert
                Assert.True(result == record.MatchId);
            }
        }

        [Fact]
        public async Task GetRecord_ReturnsMatchingRecords()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchRecordDao>>();
                var dao = new MatchRecordDao(DbConnFactory(), logger);
                var idService = new MatchIdService();
                var records = new List<MatchRecordDbo>() {
                    new MatchRecordDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Status = MatchRecordStatus.Open,
                        Invalid = false,
                        Data = "{}"
                    },
                    new MatchRecordDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Status = MatchRecordStatus.Closed,
                        Invalid = false,
                        Data = "{}"
                    }
                };

                ClearMatchRecords();
                records.ForEach(r => Insert(r));

                // Act
                var results = (await dao.GetRecords(records.First())).ToList();

                // Assert
                Assert.True(results.OrderBy(r => r.Status).SequenceEqual(records.OrderBy(r => r.Status)));
            }
        }
    }
}
