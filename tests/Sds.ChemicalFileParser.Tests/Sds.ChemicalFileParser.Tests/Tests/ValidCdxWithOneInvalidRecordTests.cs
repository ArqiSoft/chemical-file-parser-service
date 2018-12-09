using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.ChemicalFileParser.Tests
{
    public class ValidCdxWithOneInvalidRecordTestFixture
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid BlobId { get; }
        public string Bucket { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();

        public ValidCdxWithOneInvalidRecordTestFixture(ChemicalFileParserTestHarness harness)
        {
            Bucket = UserId.ToString();
            BlobId = harness.UploadBlob(Bucket, "10000_10Mos.cdx").Result;
            harness.ParseFile(Id, BlobId, Bucket, UserId, CorrelationId).Wait();
        }
    }

    [Collection("Chemical File Parser Test Harness")]
    public class ValidCdxWithOneInvalidRecordTests : ChemicalFileParserTest, IClassFixture<ValidCdxWithOneInvalidRecordTestFixture>
    {
        private Guid CorrelationId;
        private string Bucket;
        private Guid UserId;
        private Guid Id;

        public ValidCdxWithOneInvalidRecordTests(ChemicalFileParserTestHarness harness, ITestOutputHelper output, ValidCdxWithOneInvalidRecordTestFixture initFixture) : base(harness, output)
        {
            Id = initFixture.Id;
            CorrelationId = initFixture.CorrelationId;
            Bucket = initFixture.Bucket;
            UserId = initFixture.UserId;
        }

        [Fact]
        public void CdxParsing_ValidCdxFile_ShouldReceiveOneRecordParseFailedEvent()
        {
            var recordParsedEvents = Harness.GetRecordParsedEventsList(Id);
            recordParsedEvents.Should().HaveCount(2);
            foreach (var evn in recordParsedEvents)
            {
                evn.FileId.Should().Be(Id);
                evn.UserId.Should().Be(UserId);
                evn.CorrelationId.Should().Be(CorrelationId);
            }

            var recordParseFailed = Harness.GetRecordParseFailedEventsList(Id).SingleOrDefault();
            recordParseFailed.Should().NotBeNull();
            recordParseFailed.CorrelationId.Should().Be(CorrelationId);
            recordParseFailed.FileId.Should().Be(Id);
            recordParseFailed.UserId.Should().Be(UserId);

            var fileParsedEvn = Harness.GetFileParsedEvent(Id);
            fileParsedEvn.Id.Should().Be(Id);
            fileParsedEvn.UserId.Should().Be(UserId);
            fileParsedEvn.CorrelationId.Should().Be(CorrelationId);
            fileParsedEvn.FailedRecords.Should().Be(1);
            fileParsedEvn.ParsedRecords.Should().Be(2);
            fileParsedEvn.TotalRecords.Should().Be(3);

            var fileParseFailedEvn = Harness.GetFileParseFailedEvent(Id);
            fileParseFailedEvn.Should().BeNull();
        }

        [Fact]
        public async Task CdxParsing_ValidCdxFile_UploadedBlobsContainNotEmptyData()
        {
            var recordParsedEvents = Harness.GetRecordParsedEventsList(Id);
            foreach (var evn in recordParsedEvents)
            {
                var blobInfo = await Harness.BlobStorage.GetFileInfo(evn.BlobId, Bucket);
                blobInfo.Should().NotBeNull();
                blobInfo.Length.Should().BeGreaterThan(0);
                blobInfo.ContentType.ToLower().Should().BeEquivalentTo("chemical/x-mdl-molfile");
            }
        }


    }
}
