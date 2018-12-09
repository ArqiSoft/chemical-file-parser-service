using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.ChemicalFileParser.Tests
{
    public class NonSdfTestFixture
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid BlobId { get; }
        public string Bucket { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; } = Guid.NewGuid();

        public NonSdfTestFixture(ChemicalFileParserTestHarness harness)
        {
            Bucket = UserId.ToString();
            BlobId = harness.UploadBlob(Bucket, "non-mol.mol").Result;
            harness.ParseFile(Id, BlobId, Bucket, UserId, CorrelationId).Wait();
        }
    }

    [Collection("Chemical File Parser Test Harness")]
    public class NonSdfTests : ChemicalFileParserTest, IClassFixture<NonSdfTestFixture>
    {
        private Guid CorrelationId;
        private string Bucket;
        private Guid UserId;
        private Guid Id;

        public NonSdfTests(ChemicalFileParserTestHarness harness, ITestOutputHelper output, NonSdfTestFixture initFixture) : base(harness, output)
        {
            Id = initFixture.Id;
            CorrelationId = initFixture.CorrelationId;
            Bucket = initFixture.Bucket;
            UserId = initFixture.UserId;
        }

        [Fact]
        public void SdfParsing_NonSdfFile_ReceivedEventsShouldContainValidData()
        {
            var recordParsedEvent = Harness.GetRecordParsedEventsList(Id).SingleOrDefault();
            recordParsedEvent.Should().BeNull();

            var recordParseFailed = Harness.GetRecordParseFailedEventsList(Id).SingleOrDefault();
            recordParseFailed.Should().NotBeNull();
            recordParseFailed.FileId.Should().Be(Id);
            recordParseFailed.UserId.Should().Be(UserId);
            recordParseFailed.CorrelationId.Should().Be(CorrelationId);

            var fileParsedEvn = Harness.GetFileParsedEvent(Id);
            fileParsedEvn.Should().NotBeNull();
            fileParsedEvn.Id.Should().Be(Id);
            fileParsedEvn.UserId.Should().Be(UserId);
            fileParsedEvn.CorrelationId.Should().Be(CorrelationId);
            fileParsedEvn.TotalRecords.Should().Be(1);

            var fileParseFailedEvn = Harness.GetFileParseFailedEvent(Id);
            fileParseFailedEvn.Should().BeNull();
        }
    }
}
