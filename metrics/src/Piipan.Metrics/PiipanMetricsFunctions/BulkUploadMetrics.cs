// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace PiipanMetricsFunctions
{
    public static class BulkUploadMetrics
    {
        /// <summary>
        /// Listens for BulkUpload events when users upload participants;
        /// write meta info to Metrics database
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("BulkUploadMetrics")]
        public async static Task Run(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            try
            {
                string state = ParseState(eventGridEvent);
                await Write(
                    state,
                    eventGridEvent.EventTime,
                    NpgsqlFactory.Instance,
                    log
                );
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        private static string ParseState(
            [EventGridTrigger] EventGridEvent eventGridEvent
        )
        {
            var jsondata = JsonConvert.SerializeObject(eventGridEvent.Data);
            var tmp = new { url = "" };
            var data = JsonConvert.DeserializeAnonymousType(jsondata, tmp);

            Regex regex = new Regex("^https://([a-z]+)state");
            Match match = regex.Match(data.url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new FormatException("State not found");
            }
        }

        internal async static Task<string> ConnectionString()
        {
            // Environment variable (and placeholder) established
            // during initial function app provisioning in IaC
            const string DatabaseConnectionString = "DatabaseConnectionString";
            const string PasswordPlaceholder = "{password}";

            // Resource Id for open source software databases in the public Azure cloud;
            // in other clouds, see result of:
            // `az cloud show --query endpoints.ossrdbmsResourceId`
            const string ResourceId = "https://ossrdbms-aad.database.windows.net";

            var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

            if (builder.Password == PasswordPlaceholder)
            {
                var provider = new AzureServiceTokenProvider();
                var token = await provider.GetAccessTokenAsync(ResourceId);
                builder.Password = token;
            }

            return builder.ConnectionString;
        }

        internal async static Task Write(
            String state,
            DateTime uploadedAt,
            DbProviderFactory factory,
            ILogger log)
        {
            var connString = await ConnectionString();
            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                log.LogInformation("Opening db connection");
                conn.Open();
                var tx = conn.BeginTransaction();

                // Is there a way to get this db table name dynamically? Otherwise We'd have to remember to change the name in both the iac script and here.
                using (var cmd = factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO participant_uploads (state, uploaded_at) VALUES (@state, @uploaded_at)";
                    AddWithValue(cmd, DbType.String, "state", state);
                    AddWithValue(cmd, DbType.DateTime, "uploaded_at", uploadedAt);
                    int nRows = cmd.ExecuteNonQuery();
                    log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
                }
                tx.Commit();
                conn.Close();
                log.LogInformation("db connection closed");
            }
        }
    }
}
