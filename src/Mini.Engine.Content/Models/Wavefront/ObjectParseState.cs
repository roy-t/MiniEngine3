using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Parsers;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models.Wavefront;

internal record class Group(string Name, int StartFace, int EndFace)
{
    public MaterialData? Material { get; set; }
}

internal sealed class ObjectParseState : IParseState
{
    public ObjectParseState(string basePath)
    {
        this.BasePath = basePath;

        this.Vertices = new List<Vector4>(100_000);
        this.Normals = new List<Vector4>(100_000);
        this.Texcoords = new List<Vector4>(100_000);
        this.Faces = new List<Point3[]>(100_000);
        this.Groups = new List<Group>(100);
        this.Materials = new Dictionary<string, MaterialData>();

        this.Object = string.Empty;

        this.Group = null;
        this.Material = null;
    }

    public List<Vector4> Vertices { get; }
    public List<Vector4> Normals { get; }
    public List<Vector4> Texcoords { get; }
    public List<Point3[]> Faces { get; }
    public List<Group> Groups { get; }

    public Dictionary<string, MaterialData> Materials { get; }

    public string Object { get; set; }

    public Group? Group { get; set; }
    public MaterialData? Material { get; set; }

    public string BasePath { get; }

    public void EndPreviousGroup()
    {
        if (this.Group != null)
        {
            this.Groups.Add(new Group(this.Group.Name, this.Group.StartFace, this.Faces.Count - 1)
            {
                Material = this.Material
            });
        }
    }

    public void NewGroup(string name)
    {
        this.EndPreviousGroup();
        this.Group = new Group(name, this.Faces.Count, 0);
    }
}

