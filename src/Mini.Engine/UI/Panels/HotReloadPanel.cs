using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Vortice.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal class HotReloadPanel : IPanel
{
    public string Title => "Hot Reload";

    private readonly List<(DateTime, ContentId, Exception?)> Reports;

    public HotReloadPanel(ContentManager content)
    {
        this.Reports = new List<(DateTime, ContentId, Exception?)>();
        content.AddReloadReporter((c, e) => this.Reports.Insert(0, (DateTime.Now, c, e)));
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
                ImGui.Text($"{timestamp:HH::mm:ss.fff}");

                ImGui.TableSetColumnIndex(1);
                ImGui.Text(content.ToString());

                ImGui.TableSetColumnIndex(2);
                ImGui.Text(exception?.Message ?? " - ");
            }

            ImGui.EndTable();

            for (var i = 0; i < this.Reports.Count * 2; i++)
            {
                ImGui.PopStyleColor();
            }
        }


        /*
         *  const int COLUMNS_COUNT = 3;
        if (ImGui::BeginTable("table_custom_headers", COLUMNS_COUNT, ImGuiTableFlags_Borders | ImGuiTableFlags_Reorderable | ImGuiTableFlags_Hideable))
        {
            ImGui::TableSetupColumn("Apricot");
            ImGui::TableSetupColumn("Banana");
            ImGui::TableSetupColumn("Cherry");

            // Dummy entire-column selection storage
            // FIXME: It would be nice to actually demonstrate full-featured selection using those checkbox.
            static bool column_selected[3] = {};

            // Instead of calling TableHeadersRow() we'll submit custom headers ourselves
            ImGui::TableNextRow(ImGuiTableRowFlags_Headers);
            for (int column = 0; column < COLUMNS_COUNT; column++)
            {
                ImGui::TableSetColumnIndex(column);
                const char* column_name = ImGui::TableGetColumnName(column); // Retrieve name passed to TableSetupColumn()
                ImGui::PushID(column);
                ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(0, 0));
                ImGui::Checkbox("##checkall", &column_selected[column]);
                ImGui::PopStyleVar();
                ImGui::SameLine(0.0f, ImGui::GetStyle().ItemInnerSpacing.x);
                ImGui::TableHeader(column_name);
                ImGui::PopID();
            }

            for (int row = 0; row < 5; row++)
            {
                ImGui::TableNextRow();
                for (int column = 0; column < 3; column++)
                {
                    char buf[32];
                    sprintf(buf, "Cell %d,%d", column, row);
                    ImGui::TableSetColumnIndex(column);
                    ImGui::Selectable(buf, column_selected[column]);
                }
            }
            ImGui::EndTable();
        }
        ImGui::TreePop();
    }
         */
    }
}
