﻿namespace Mini.Engine.Content.Generators.Source
{
    public sealed class Struct : Type
    {
        public Struct(string name, params string[] modifiers)
          : base(name, modifiers) { }

        public override string TypeKeyword => "struct";
    }
}