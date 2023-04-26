using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Vortice.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal class HotReloadPanel : IEditorPanel, IDieselPanel
{
    public string Title => "Hot Reload";

    private readonly List<(DateTime, string, Exception?)> Reports;

    public HotReloadPanel(ContentManager content)
    {
        this.Reports = new List<(DateTime, string, Exception?)>();
        content.AddReloadReporter((c, e) => this.Reports.Insert(0, (DateTime.Now, c.ToString(), e)));

#if DEBUG
        HotReloadManager.AddReloadReporter(f => this.Reports.Insert(0, (DateTime.Now, f, null)));
#endif
    }

    public void Update(float elapsed)
    {
        if (ImGui.BeginTable("Reports", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable))
        {
            ImGui.TableSetupColumn("TimeStamp", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Content", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Exception", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            for (var i = 0; i < this.Reports.Count; i++)
            {
                var report = this.Reports[i];
                var timestamp = report.Item1;
                var content = report.Item2;
                var exception = report.Item3;

                ImGui.TableNextRow();

                if (exception != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.TableRowBg, Colors.DarkRed.ToVector4());
                    ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, Colors.DarkRed.ToVector4());
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.TableRowBg, Colors.DarkGreen.ToVector4());
                    ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, Colors.DarkGreen.ToVector4());
                }

                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted($"{timestamp:HH::mm:ss.fff}");

                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(content);

                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(exception?.Message ?? " - ");
            }

            ImGui.EndTable();

            for (var i = 0; i < this.Reports.Count * 2; i++)
            {
                ImGui.PopStyleColor();
            }
        }
    }
}
