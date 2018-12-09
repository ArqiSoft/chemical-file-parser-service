using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.ChemicalFileParser.Tests
{
    public class ValidMolTestFixture
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid BlobId { get; }
        public string Bucket { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();

        public ValidMolTestFixture(ChemicalFileParserTestHarness harness)
        {
            Bucket = UserId.ToString();
            BlobId = harness.UploadBlob(Bucket, "Aspirin.mol").Result;
            harness.ParseFile(Id, BlobId, Bucket, UserId, CorrelationId).Wait();
        }
    }

    [Collection("Chemical File Parser Test Harness")]
    public class ValidMolTests : ChemicalFileParserTest, IClassFixture<ValidMolTestFixture>
    {
        private Guid CorrelationId;
        private string Bucket;
        private Guid UserId;
        private Guid Id;

        public ValidMolTests(ChemicalFileParserTestHarness harness, ITestOutputHelper output, ValidMolTestFixture initFixture) : base(harness, output)
        {
            Id = initFixture.Id;
            CorrelationId = initFixture.CorrelationId;
            Bucket = initFixture.Bucket;
            UserId = initFixture.UserId;
        }

        [Fact]
        public void MolParsing_ValidMolFile_ReceivedEventsShouldContainValidData()
        {
            var recordParsedEvent = Harness.GetRecordParsedEventsList(Id).SingleOrDefault();
            recordParsedEvent.Should().NotBeNull();
            recordParsedEvent.FileId.Should().Be(Id);
            recordParsedEvent.UserId.Should().Be(UserId);
            recordParsedEvent.CorrelationId.Should().Be(CorrelationId);

            var recordParseFailed = Harness.GetRecordParseFailedEventsList(Id);
            recordParseFailed.Should().HaveCount(0);

            var fileParsedEvn = Harness.GetFileParsedEvent(Id);
            fileParsedEvn.Id.Should().Be(Id);
            fileParsedEvn.UserId.Should().Be(UserId);
            fileParsedEvn.CorrelationId.Should().Be(CorrelationId);
            fileParsedEvn.FailedRecords.Should().Be(0);
            fileParsedEvn.ParsedRecords.Should().Be(1);
            fileParsedEvn.TotalRecords.Should().Be(1);

            var fileParseFailedEvn = Harness.GetFileParseFailedEvent(Id);
            fileParseFailedEvn.Should().BeNull();
        }

        [Fact]
        public async Task MolParsing_ValidMolFile_UploadedBlobsContainNotEmptyData()
        {
            var recordParsedEvent = Harness.GetRecordParsedEventsList(Id).SingleOrDefault();
            recordParsedEvent.Should().NotBeNull();
            var blobInfo = await Harness.BlobStorage.GetFileInfo(recordParsedEvent.BlobId, Bucket);
            blobInfo.Should().NotBeNull();
            blobInfo.Length.Should().BeGreaterThan(0);
            blobInfo.ContentType.ToLower().Should().BeEquivalentTo("chemical/x-mdl-molfile");
        }
    }
}
