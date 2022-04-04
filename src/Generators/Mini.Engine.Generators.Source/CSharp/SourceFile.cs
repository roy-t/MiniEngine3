namespace Mini.Engine.Generators.Source.CSharp;

public sealed class Namespace
{
    public string Pattern =
@"
namespace %name%;

%struct+%

%class+%
";
}




public sealed class SourceFile
{
    public string Pattern =
$@"
using %using+%
%namespace%
";


}