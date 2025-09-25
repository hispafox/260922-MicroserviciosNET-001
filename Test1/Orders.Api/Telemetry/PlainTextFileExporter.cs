using OpenTelemetry.Trace;
using OpenTelemetry;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Orders.Api.Telemetry
{
    public class PlainTextFileExporter : BaseExporter<Activity>
    {
        private readonly string _filePath;
        private readonly object _lock = new();

        public PlainTextFileExporter(string filePath)
        {
            _filePath = filePath;
        }

        public override ExportResult Export(in Batch<Activity> batch)
        {
            lock (_lock)
            {
                using var writer = new StreamWriter(_filePath, append: true, Encoding.UTF8);
                foreach (var activity in batch)
                {
                    writer.WriteLine($"Span: {activity.DisplayName}");
                    writer.WriteLine($"TraceId: {activity.TraceId}");
                    writer.WriteLine($"SpanId: {activity.SpanId}");
                    writer.WriteLine($"Start: {activity.StartTimeUtc}");
                    writer.WriteLine($"Duration: {activity.Duration}");
                    writer.WriteLine($"Status: {activity.Status}");
                    writer.WriteLine($"StatusDescription: {activity.StatusDescription}");
                    writer.WriteLine("Tags:");
                    foreach (var tag in activity.Tags)
                    {
                        writer.WriteLine($"  {tag.Key}: {tag.Value}");
                    }
                    writer.WriteLine(new string('-', 40));
                }
            }
            return ExportResult.Success;
        }
    }
}
