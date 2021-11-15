using System.Collections.Generic;
using System.Numerics;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models;

internal record class Group(string Name, int StartFace, int EndFace);

internal sealed class ParseState
{
    public List<Vector4> Vertices { get; }
    public List<Vector4> Normals { get; }
    public List<Vector4> Texcoords { get; }
    public List<Point3[]> Faces { get; }
    public List<Group> Groups { get; }

    public List<string> MaterialLibraries { get; }

    public string Object { get; set; }
    public string Material { get; set; }

    public Group? Group { get; set; }

    public ParseState()
    {
        this.Vertices = new List<Vector4>(100_000);
        this.Normals = new List<Vector4>(100_000);
        this.Texcoords = new List<Vector4>(100_000);
        this.Faces = new List<Point3[]>(100_000);
        this.Groups = new List<Group>(100);
        this.MaterialLibraries = new List<string>(1);

        this.Group = null;
        this.Object = string.Empty;
        this.Material = string.Empty;
    }

    public void EndPreviousGroup()
    {
        if (this.Group != null)
        {
            this.Groups.Add(new Group(this.Group.Name, this.Group.StartFace, this.Faces.Count - 1));
        }
    }

    public void NewGroup(string name)
    {
        this.EndPreviousGroup();
        this.Group = new Group(name, this.Faces.Count, 0);
    }
}

