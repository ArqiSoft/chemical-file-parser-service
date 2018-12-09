using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.ChemicalFileParser.Tests
{
    public class ValidSdfTestFixture
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid BlobId { get; }
        public string Bucket { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();

        public ValidSdfTestFixture(ChemicalFileParserTestHarness harness)
        {
            Bucket = UserId.ToString();
            BlobId = harness.UploadBlob(Bucket, "DrugBank_10.sdf").Result;
            harness.ParseFile(Id, BlobId, Bucket, UserId, CorrelationId).Wait();
        }
    }

    [Collection("Chemical File Parser Test Harness")]
    public class ValidSdfTests : ChemicalFileParserTest, IClassFixture<ValidSdfTestFixture>
    {
        private Guid CorrelationId;
        private string Bucket;
        private Guid UserId;
        private Guid Id;

        public ValidSdfTests(ChemicalFileParserTestHarness harness, ITestOutputHelper output, ValidSdfTestFixture initFixture) : base(harness, output)
        {
            Id = initFixture.Id;
            CorrelationId = initFixture.CorrelationId;
            Bucket = initFixture.Bucket;
            UserId = initFixture.UserId;
        }

        [Fact]
        public void SdfParsing_ValidSdfFile_ReceivedEventsShouldContainValidData()
        {
            var recordParsedEvents = Harness.GetRecordParsedEventsList(Id);
            recordParsedEvents.Should().HaveCount(10);
            foreach (var evn in recordParsedEvents)
            {
                evn.FileId.Should().Be(Id);
                evn.UserId.Should().Be(UserId);
                evn.CorrelationId.Should().Be(CorrelationId);
            }

            var recordParseFailed = Harness.GetRecordParseFailedEventsList(Id);
            recordParseFailed.Should().HaveCount(0);

            var fileParsedEvn = Harness.GetFileParsedEvent(Id);
            fileParsedEvn.Id.Should().Be(Id);
            fileParsedEvn.UserId.Should().Be(UserId);
            fileParsedEvn.CorrelationId.Should().Be(CorrelationId);
            fileParsedEvn.FailedRecords.Should().Be(0);
            fileParsedEvn.ParsedRecords.Should().Be(10);
            fileParsedEvn.TotalRecords.Should().Be(10);

            var fileParseFailedEvn = Harness.GetFileParseFailedEvent(Id);
            fileParseFailedEvn.Should().BeNull();
        }

        [Fact]
        public async Task SdfParsing_ValidSdfFile_UploadedBlobsContainNotEmptyData()
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
