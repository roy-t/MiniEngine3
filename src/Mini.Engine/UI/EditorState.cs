using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics;
using Mini.Engine.Scenes;

namespace Mini.Engine.UI;

[Service]
public class EditorState
{
    private const string Path = "editor.blob";
    private readonly FrameService FrameService;
    private readonly SceneManager SceneManager;
    
    private Transform preferredTransform = Transform.Identity;

    private bool shouldUpdate = false;

    public EditorState(FrameService frameService, SceneManager sceneManager)
    {
        this.FrameService = frameService;
        this.SceneManager = sceneManager;
    }

    public int PreferredScene { get; private set; }

    public void Save()
    {
        try
        {
            using var stream = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan);
            using var writer = new BinaryWriter(stream);
            writer.Write(this.SceneManager.ActiveScene);

            ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

            // Position
            writer.Write(cameraTransform.Current.GetPosition().X);
            writer.Write(cameraTransform.Current.GetPosition().Y);
            writer.Write(cameraTransform.Current.GetPosition().Z);

            // Rotation
            writer.Write(cameraTransform.Current.GetRotation().X);
            writer.Write(cameraTransform.Current.GetRotation().Y);
            writer.Write(cameraTransform.Current.GetRotation().Z);
            writer.Write(cameraTransform.Current.GetRotation().W);

            // Origin
            writer.Write(cameraTransform.Current.GetOrigin().X);
            writer.Write(cameraTransform.Current.GetOrigin().Y);
            writer.Write(cameraTransform.Current.GetOrigin().Z);

            // Scale
            writer.Write(cameraTransform.Current.GetScale());
        }
        catch (Exception) { }
    }

    public void Restore()
    {        
        try
        {
            using var stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.SequentialScan);
            using var reader = new BinaryReader(stream);

            this.PreferredScene = reader.ReadInt32();

            // Position
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            // Rotation
            var rx = reader.ReadSingle();
            var ry = reader.ReadSingle();
            var rz = reader.ReadSingle();
            var rw = reader.ReadSingle();

            // Origin
            var ox = reader.ReadSingle();
            var oy = reader.ReadSingle();
            var oz = reader.ReadSingle();

            // Scale
            var s = reader.ReadSingle();

            this.preferredTransform = new Transform(new Vector3(x, y, z), new Quaternion(rx, ry, rz, rw), new Vector3(ox, oy, oz), s);

            this.shouldUpdate = true;
        }
        catch (Exception)
        {
            this.PreferredScene = 0;
            this.preferredTransform = Transform.Identity;
            this.shouldUpdate = false;
        }
    }

    public void Update()
    {
        if (this.shouldUpdate)
        {            
            ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();
            cameraTransform.Current = this.preferredTransform;

            this.shouldUpdate = false;
        }
    }
}
