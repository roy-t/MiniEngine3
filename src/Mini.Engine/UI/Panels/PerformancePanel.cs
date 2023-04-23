using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PerformancePanel : IDieselPanel
{
    public string Title => "Performance";

    private readonly MetricService MetricService;
    private readonly PerformanceCounters Counters;
    private readonly List<Gauge> OrderedGauges;

    private string search;


    public PerformancePanel(PerformanceCounters counters, MetricService metricService)
    {
        this.Counters = counters;
        this.MetricService = metricService;

        this.OrderedGauges = new List<Gauge>();

        this.search = string.Empty;
    }

    public void Update(float elapsed)
    {
        this.MetricService.Update("Host.GPU.MemoryUsage.MB", this.Counters.GPUMemoryCounter.Value / (1024 * 1024));
        this.MetricService.Update("Host.GPU.Usage.%", this.Counters.GPUUsageCounter.Value);
        this.MetricService.Update("Host.CPU.MemoryUsage.MB", this.Counters.CPUMemoryCounter.Value / (1024 * 1024));
        this.MetricService.Update("Host.CPU.Usage.%", this.Counters.CPUUsageCounter.Value);

        ImGui.InputText("Filter", ref this.search, 100);

        if (ImGui.BeginTable("Gauges", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable))
        {
            ImGui.TableSetupColumn("Tag", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Min", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Max", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Average", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableHeadersRow();

            this.OrderedGauges.Clear();
            foreach (var gauge in this.MetricService.Gauges)
            {
                if (string.IsNullOrEmpty(this.search) || gauge.Tag.StartsWith(this.search, StringComparison.OrdinalIgnoreCase))
                {
                    this.OrderedGauges.Add(gauge);
                }
            }

            foreach (var gauge in this.OrderedGauges.OrderBy(g => g.Tag))
            {
                ImGui.TableNextRow();
                var (average, min, max) = this.MetricService.Statistics(in gauge);

                var parts = gauge.Tag.Split('.');

                var unit = parts.LastOrDefault(string.Empty);

                var name = gauge.Tag;
                if (unit != string.Empty)
                {
                    name = gauge.Tag.Substring(0, name.Length - unit.Length - 1);
                    if (unit == "%") { unit = "%%"; }
                }
                
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(name);

                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"{min:000.00} {unit}");

                ImGui.TableSetColumnIndex(2);
                ImGui.Text($"{max:000.00} {unit}");

                ImGui.TableSetColumnIndex(3);
                ImGui.Text($"{average:000.00} {unit}");
            }

            ImGui.EndTable();
        }
    }
}
