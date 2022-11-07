using System.Text;
using Mini.Engine.Content.Parsers;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2.Models.Wavefront;

/// <summary>
/// Group name statements are used to organize collections of elements and simplify data manipulation for operations in model.
/// syntax: g group_name1 group_name2 ...
/// </summary>
internal sealed class GroupParser : ObjStatementParser
{
    public override string Key => "g";

    protected override void ParseArguments(ParseState state, SpanTokenEnumerator arguments, IReadOnlyVirtualFileSystem fileSystem)
    {
        var builder = new StringBuilder();
        foreach (var name in arguments)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }
            builder.Append(name);

        }
        state.NewGroup(builder.ToString());
    }
}
