using System;
using System.Collections.Generic;
using System.Text;

namespace Mini.Engine.Generators.Source.CSharp;


public sealed class Namespace
{
    public string Pattern =
@"
%repeat% using {using};

namespace {name};
";
}




public sealed class SourceFile
{
    public string Pattern =
$@"
public 
";


}