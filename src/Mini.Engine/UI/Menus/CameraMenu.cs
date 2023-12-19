using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics;

namespace Mini.Engine.UI.Menus;

[Service]
internal sealed class CameraMenu : IEditorMenu
{
    private readonly FrameService FrameService;
    private readonly float[] DistanceOptions;

    public CameraMenu(FrameService frameService)
    {
        this.FrameService = frameService;
        this.DistanceOptions = new float[] { 10.0f, 25.0f, 50.0f, 100.0f, 250.0f };
    }

    public string Title => "Camera";
    
    public void Update()
    {
        if(ImGui.BeginMenu("Birds Eye"))
        {
            for(var i = 0; i < this.DistanceOptions.Length; i++)
            {
                var option = this.DistanceOptions[i];
                if (ImGui.MenuItem($"{option}m"))
                {
                    ref var transform = ref this.FrameService.GetPrimaryCameraTransform();
                    transform.Current = new Transform(Vector3.UnitY * option, Quaternion.Identity, Vector3.Zero, 1.0f)
                    .FaceTarget(Vector3.UnitZ * 0.001f);
                }
            }
            
            ImGui.EndMenu();
        }
    }
}
