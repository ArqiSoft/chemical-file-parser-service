using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.ChemicalFileParser.Tests
{
    public class NonCdxTestFixture
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid BlobId { get; }
        public string Bucket { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();

        public NonCdxTestFixture(ChemicalFileParserTestHarness harness)
        {
            Bucket = UserId.ToString();
            BlobId = harness.UploadBlob(Bucket, "non-cdx.cdx").Result;
            harness.ParseFile(Id, BlobId, Bucket, UserId, CorrelationId).Wait();
        }
    }

    [Collection("Chemical File Parser Test Harness")]
    public class NonCdxTests : ChemicalFileParserTest, IClassFixture<NonCdxTestFixture>
    {
        private Guid CorrelationId;
        private string Bucket;
        private Guid UserId;
        private Guid Id;

        public NonCdxTests(ChemicalFileParserTestHarness harness, ITestOutputHelper output, NonCdxTestFixture initFixture) : base(harness, output)
        {
            Id = initFixture.Id;
            CorrelationId = initFixture.CorrelationId;
            Bucket = initFixture.Bucket;
            UserId = initFixture.UserId;
        }

        [Fact]
        public void CdxParsing_NonCdxFile_ReceivedEventsShouldContainValidData()
        {
            var recordParsedEvent = Harness.GetRecordParsedEventsList(Id).SingleOrDefault();
            recordParsedEvent.Should().BeNull();

            var recordParseFailed = Harness.GetRecordParseFailedEventsList(Id).SingleOrDefault();
            recordParseFailed.Should().BeNull();

            var fileParsedEvn = Harness.GetFileParsedEvent(Id);
            fileParsedEvn.Should().NotBeNull();
            fileParsedEvn.Id.Should().Be(Id);
            fileParsedEvn.UserId.Should().Be(UserId);
            fileParsedEvn.CorrelationId.Should().Be(CorrelationId);
            fileParsedEvn.TotalRecords.Should().Be(0);

            var fileParseFailedEvn = Harness.GetFileParseFailedEvent(Id);
            fileParseFailedEvn.Should().BeNull();
        }
    }
}
