﻿using Mini.Engine.Configuration;
using Mini.Engine.UI.Menus;
using Mini.Engine.UI.Panels;

namespace Mini.Engine.UI;

[Service]
internal sealed class EditorUserInterface : UserInterface
{
    public EditorUserInterface(UICore core, IEnumerable<IEditorPanel> panels, IEnumerable<IEditorMenu> menus)
        : base(core, panels, menus) { }
}
