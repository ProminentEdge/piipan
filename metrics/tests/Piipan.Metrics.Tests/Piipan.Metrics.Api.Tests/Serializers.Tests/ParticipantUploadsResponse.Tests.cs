// using System;
// using System.Collections.Generic;
// using System.Data.Common;
// using Xunit;
// using Newtonsoft.Json;
// using Piipan.Metrics.Models;
// using Piipan.Metrics.Api.Serializers;

// namespace Piipan.Metrics.Tests
// {
//     namespace Piipan.Metrics.Api.Tests
//     {
//         public class ParticipantUploadsResponseTests
//         {
//             [Fact]
//             public static void CreateSuccess()
//             {
//                 var participant_upload = new ParticipantUpload();
//                 participant_upload.state = "foobar";
//                 participant_upload.uploaded_at = new DateTime();
//                 List<ParticipantUpload> list = new List<ParticipantUpload>() { participant_upload };
//                 var meta = new Meta();
//                 meta.page = 1;
//                 meta.perPage = 5;
//                 meta.total = 6;
//                 var result = new ParticipantUploadsResponse(
//                     list,
//                     meta
//                 );
//                 Assert.Equal(list, result.data);
//                 Assert.Equal(6, result.meta.total);
//             }
//             // Just to want to view the serialized result
//             [Fact]
//             public static void SerializeSuccess()
//             {
//                 var participant_upload = new ParticipantUpload();
//                 participant_upload.state = "foobar";
//                 participant_upload.uploaded_at = new DateTime();
//                 List<ParticipantUpload> list = new List<ParticipantUpload>() { participant_upload };
//                 var meta = new Meta();
//                 meta.page = 1;
//                 meta.perPage = 5;
//                 meta.total = 6;
//                 var result = new ParticipantUploadsResponse(
//                     list,
//                     meta
//                 );
//                 var serializedResult = JsonConvert.SerializeObject(result, Formatting.Indented);
//                 Console.WriteLine(serializedResult);
//                 Assert.Equal("".GetType(), serializedResult.GetType());
//             }
//         }
//     }
// }
