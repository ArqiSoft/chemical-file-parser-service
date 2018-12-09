using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.ChemicalFileParser.Tests
{
    public class EmptyCdxTestFixture
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid BlobId { get; }
        public string Bucket { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();

        public EmptyCdxTestFixture(ChemicalFileParserTestHarness harness)
        {
            Bucket = UserId.ToString();
            BlobId = harness.UploadBlob(Bucket, "empty.cdx").Result;
            harness.ParseFile(Id, BlobId, Bucket, UserId, CorrelationId).Wait();
        }
    }

    [Collection("Chemical File Parser Test Harness")]
    public class EmptyCdxTests : ChemicalFileParserTest, IClassFixture<EmptyCdxTestFixture>
    {
        private Guid CorrelationId;
        private string Bucket;
        private Guid UserId;
        private Guid Id;

        public EmptyCdxTests(ChemicalFileParserTestHarness harness, ITestOutputHelper output, EmptyCdxTestFixture initFixture) : base(harness, output)
        {
            Id = initFixture.Id;
            CorrelationId = initFixture.CorrelationId;
            Bucket = initFixture.Bucket;
            UserId = initFixture.UserId;
        }

        [Fact]
        public void SdfParsing_EmptySdfFile_ReceivedEventsShouldContainValidData()
        {
            var recordParsedEvents = Harness.GetRecordParsedEventsList(Id);
            recordParsedEvents.Should().HaveCount(0);

            var recordParseFailed = Harness.GetRecordParseFailedEventsList(Id);
            recordParseFailed.Should().HaveCount(0);

            var fileParsedEvn = Harness.GetFileParsedEvent(Id);
            fileParsedEvn.Should().NotBeNull();
            fileParsedEvn.Id.Should().Be(Id);
            fileParsedEvn.UserId.Should().Be(UserId);
            fileParsedEvn.CorrelationId.Should().Be(CorrelationId);
            fileParsedEvn.FailedRecords.Should().Be(0);
            fileParsedEvn.ParsedRecords.Should().Be(0);
            fileParsedEvn.ParsedRecords.Should().Be(0);

            var fileParseFailedEvn = Harness.GetFileParseFailedEvent(Id);
            fileParseFailedEvn.Should().BeNull();
        }
    }
}
