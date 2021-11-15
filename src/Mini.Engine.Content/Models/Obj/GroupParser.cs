using System.Text;

namespace Mini.Engine.Content.Models.Obj;

/// <summary>
/// Group name statements are used to organize collections of elements and simplify data manipulation for operations in model.
/// syntax: g group_name1 group_name2 ...
/// </summary>
internal sealed class GroupParser : StatementParser
{
    public override string Key => "g";

    protected override void ParseArguments(ParseState state, SpanTokenEnumerator arguments)
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
