using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Match.Client.Tests
{
    public class MatchClientTests
    {
        [Fact]
        public async Task FindMatches_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new OrchMatchResponse
            {
                Data = new OrchMatchResponseData
                {
                    Results = new List<OrchMatchResult>
                    {
                        new OrchMatchResult { Index = 0, Matches = new List<IParticipant> { Mock.Of<IParticipant>() }}
                    }
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<MatchClient>>();
            apiClient
                .Setup(m => m.PostAsync<OrchMatchRequest, OrchMatchResponse>("find_matches", It.IsAny<OrchMatchRequest>()))
                .ReturnsAsync(expectedResponse);

            var client = new MatchClient(apiClient.Object);

            var request = new OrchMatchRequest()
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson() { LdsHash = "hash1" },
                    new RequestPerson() { LdsHash = "hash2" }
                }
            };

            // Act
            var response = await client.FindMatches(request);

            // Assert
            Assert.Equal(expectedResponse, response);
        }
    }
}
