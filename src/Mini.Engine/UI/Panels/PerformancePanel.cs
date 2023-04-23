using System.Drawing.Text;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.Graphics.Diesel;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PerformancePanel : IEditorPanel, IDieselPanel
{
    public string Title => "Performance";

    private readonly MetricService MetricService;
    private readonly List<Gauge> FilteredGauges;

    private string search;


    public PerformancePanel(MetricService metricService)
    {
        this.MetricService = metricService;
        this.FilteredGauges = new List<Gauge>();
        this.search = string.Empty;
    }

    public void Update(float elapsed)
    {
        this.MetricService.UpdateBuiltInGauges();

        ImGui.InputText("Filter", ref this.search, 100);

        if (ImGui.BeginTable("Gauges", 4,  ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable))
        {
            ImGui.TableSetupColumn("Tag", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Min", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Max", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Average", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableHeadersRow();

            this.FilteredGauges.Clear();
            foreach (var gauge in this.MetricService.Gauges)
            {
                if (string.IsNullOrEmpty(this.search) || gauge.Tag.StartsWith(this.search, StringComparison.OrdinalIgnoreCase))
                {
                    this.FilteredGauges.Add(gauge);
                }
            }

            foreach (var gauge in this.FilteredGauges.OrderBy(g => g.Tag))
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted(gauge.Tag);

                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(Format(gauge.Min));

                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(Format(gauge.Max));

                ImGui.TableSetColumnIndex(3);
                ImGui.TextUnformatted(Format(gauge.Average));
            }

            ImGui.EndTable();
        }
    }

    private static string Format(float value)
    {
        return string.Format("{0, 6}", $"{value:F2}");
    }
}
