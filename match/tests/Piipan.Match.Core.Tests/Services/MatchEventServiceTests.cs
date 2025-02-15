using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Participants.Api.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchEventServiceTests
    {
        private Mock<IActiveMatchRecordBuilder> BuilderMock(MatchRecordDbo record)
        {
            var recordBuilder = new Mock<IActiveMatchRecordBuilder>();
            recordBuilder
                .Setup(r => r.SetMatch(It.IsAny<RequestPerson>(), It.IsAny<IParticipant>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.SetStates(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.GetRecord())
                .Returns(record);

            return recordBuilder;
        }

        private Mock<IMatchRecordApi> ApiMock(string matchId = "foo")
        {
            var api = new Mock<IMatchRecordApi>();
            api.Setup(r => r.AddRecord(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(matchId);

            return api;
        }

        [Fact]
        public async void Resolve_AddsSingleRecord()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(
                It.Is<IMatchRecord>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Once);
        }

        [Fact]
        public async void Resolve_AddsManyRecords()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() {
                    new ParticipantMatch { LdsHash = "foo", State= "eb" },
                    new ParticipantMatch { LdsHash = "foo", State = "ec" }
                }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(
                It.Is<IMatchRecord>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Exactly(result.Matches.Count()));
        }

        [Fact]
        public async void Resolve_InsertsMatchId()
        {
            // Arrange
            var mockMatchId = "BDC2345";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock(mockMatchId);

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            Assert.Equal(mockMatchId, resolvedResponse.Data.Results.First().Matches.First().MatchId);
        }

        [Fact]
        public async void Resolve_InsertsMostRecentMatchId()
        {
            // Arrange
            var openMatchId = "BDC2345";
            var closedMatchId = "CDB5432";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(new List<MatchRecordDbo> {
                    new MatchRecordDbo {
                        MatchId = openMatchId,
                        Status = MatchRecordStatus.Open,
                        CreatedAt = new DateTime(2020,01,02)
                    },
                    new MatchRecordDbo {
                        MatchId = closedMatchId,
                        Status = MatchRecordStatus.Open,
                        CreatedAt = new DateTime(2020,01,01)
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            Assert.Equal(openMatchId, resolvedResponse.Data.Results.First().Matches.First().MatchId);
        }

        [Fact]
        public async void Resolve_InsertsOpenMatchId()
        {
            // Arrange
            var openMatchId = "BDC2345";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(new List<MatchRecordDbo> {
                    new MatchRecordDbo {
                        MatchId = openMatchId,
                        Status = MatchRecordStatus.Open
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            Assert.Equal(openMatchId, resolvedResponse.Data.Results.First().Matches.First().MatchId);
        }

        [Fact]
        public async void Resolve_InsertsNewMatchIdIfMostRecentRecordIsClosed()
        {
            // Arrange
            var newId = "newId";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock(newId);
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(new List<MatchRecordDbo> {
                    new MatchRecordDbo {
                        MatchId = "closedId",
                        Status = MatchRecordStatus.Closed,
                        CreatedAt = new DateTime(2020,01,02)
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            Assert.Equal(newId, resolvedResponse.Data.Results.First().Matches.First().MatchId);
        }
    }
}
