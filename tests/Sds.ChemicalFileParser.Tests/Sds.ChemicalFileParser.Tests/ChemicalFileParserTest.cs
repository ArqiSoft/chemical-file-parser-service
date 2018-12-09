using MassTransit;
using Sds.Storage.Blob.Core;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Sds.ChemicalFileParser.Tests
{
    [CollectionDefinition("Chemical File Parser Test Harness")]
    public class OsdrTestCollection : ICollectionFixture<ChemicalFileParserTestHarness>
    {
    }

    public abstract class ChemicalFileParserTest
    {
        public ChemicalFileParserTestHarness Harness { get; }

        protected IBus Bus => Harness.BusControl;
        protected IBlobStorage BlobStorage => Harness.BlobStorage;

        public ChemicalFileParserTest(ChemicalFileParserTestHarness fixture, ITestOutputHelper output = null)
        {
            Harness = fixture;

            if (output != null)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo
                    .TestOutput(output, LogEventLevel.Verbose)
                    .CreateLogger()
                    .ForContext<ChemicalFileParserTest>();
            }
        }
    }
}
