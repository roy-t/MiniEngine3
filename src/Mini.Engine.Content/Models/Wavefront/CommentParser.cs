namespace Mini.Engine.Content.Models.Wavefront;

/// <summary>
/// Comment, ignored by the parser
/// syntax: # my comment
/// </summary>
internal sealed class CommentParser : ObjStatementParser
{
    public override string Key => "#";
}
