using OpenFeature.Contrib.Providers.Flagd;
using OpenFeature.Contrib.Hooks.Otel;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace OpenFeatureTestApp
{
    class Hello {
        static async Task Main(string[] args) {
            // This example assumes that the flagd server is running locally
            // For example, you can start flagd with the following example configuration:
            // flagd start --uri https://raw.githubusercontent.com/open-feature/flagd/main/config/samples/example_flags.json

                        // List that will be populated with the traces by InMemoryExporter
            var exportedItems = new List<System.Diagnostics.Activity>();

            // Create a new in-memory exporter
            //var exporter = new JaegerExporter<Activity>(exportedItems);

            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddSource("my-tracer")
                    .ConfigureResource(r => r.AddService("jaeger-test"))
                    .AddOtlpExporter(o => 
                    {
                        o.ExportProcessorType = ExportProcessorType.Simple;
                    })
                    .Build();

            Environment.SetEnvironmentVariable("FLAGD_CACHE", "LRU");
            Environment.SetEnvironmentVariable("FLAGD_MAX_EVENT_STREAM_RETRIES", "10");
            //var flagdProvider = new FlagdProvider(new Uri("http://localhost:8013"));
            var flagdProvider = new FlagdProvider();

            System.Console.WriteLine(Directory.GetCurrentDirectory());

            //var flagdProvider = new FlagdProvider();

            // set the flagdProvider as the provider for the OpenFeature SDK
            OpenFeature.Api.Instance.SetProvider(flagdProvider);

            var client = OpenFeature.Api.Instance.GetClient("my-app");

            //OpenFeature.Api.Instance.AddHooks(new OtelHook());
            await Task.Delay(200);
            try {
                var span = tracerProvider.GetTracer("my-tracer").StartActiveSpan("feature-flag-retrieval");
                var val = client.GetBooleanValue("myBoolFlag", false, null);
                // Print the value of the 'myBoolFlag' feature flag
                System.Console.WriteLine(val.Result.ToString());
                span.End();

                val = client.GetBooleanValue("myBoolFlag", false, null);
                // Print the value of the 'myBoolFlag' feature flag
                System.Console.WriteLine(val.Result.ToString());

                await Task.Delay(20000);
                val = client.GetBooleanValue("myBoolFlag", false, null);
                // Print the value of the 'myBoolFlag' feature flag
                System.Console.WriteLine(val.Result.ToString());
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }    
        }
    }
}
