using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.Content.Parsers;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models.Wavefront;

internal record class Group(string Name, int StartFace, int EndFace)
{
    public string? Material { get; set; }
}

internal sealed class ParseState : IParseState
{
    public ParseState()
    {
        this.Positions = new List<Vector4>(100_000);
        this.Normals = new List<Vector4>(100_000);
        this.Texcoords = new List<Vector4>(100_000);
        this.Faces = new List<Int3[]>(100_000);
        this.Groups = new List<Group>(100);
        this.MaterialLibrary = string.Empty;
        this.Object = string.Empty;

        this.Group = null;
        this.Material = null;
    }

    public List<Vector4> Positions { get; }
    public List<Vector4> Normals { get; }
    public List<Vector4> Texcoords { get; }
    public List<Int3[]> Faces { get; }
    public List<Group> Groups { get; }

    public string Object { get; set; }

    public Group? Group { get; set; }
    public string? Material { get; set; }
    public string MaterialLibrary { get; internal set; }

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
        this.Material = string.Empty;
    }
}

