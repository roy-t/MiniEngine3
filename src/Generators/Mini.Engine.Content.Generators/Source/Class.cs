namespace Mini.Engine.Content.Generators.Source
{
    public sealed class Class : Type
    {
        public Class(string name, params string[] modifiers)
            : base(name, modifiers) { }

        public override string TypeKeyword => "class";
    }
}
