namespace Mini.Engine.Content.Models.Obj;

/// <summary>
/// Comment, ignored by the parser
/// syntax: # my comment
/// </summary>
internal sealed class CommentParser : StatementParser
{
    public override string Key => "#";
}
