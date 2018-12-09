using Sds.ChemicalFileParser.Domain;
using Sds.ChemicalFileParser.Domain.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sds.ChemicalFileParser.Tests
{
    public static class ChemicalFileParserTestHarnessExtensions
    {
        public static async Task<Guid> UploadBlob(this ChemicalFileParserTestHarness harness, string bucket, string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName);
            var source = new FileStream(filePath, FileMode.Open);
            return await harness.BlobStorage.AddFileAsync(fileName, source, "application/octet-stream", bucket);
        }

        public static async Task ParseFile(this ChemicalFileParserTestHarness harness, Guid id, Guid blobId, string bucket, Guid userId, Guid correlationId)
        {
            await harness.BusControl.Publish<ParseFile>(new
            {
                Id = id,
                UserId = userId,
                BlobId = blobId,
                Bucket = bucket,
                CorrelationId = correlationId
            });

            if (!harness.Received.Select<FileParsed>(m => m.Context.Message.CorrelationId == correlationId).Any() && !harness.Received.Select<FileParseFailed>(m => m.Context.Message.CorrelationId == correlationId).Any())
            {
                throw new TimeoutException();
            }
        }
    }
}
