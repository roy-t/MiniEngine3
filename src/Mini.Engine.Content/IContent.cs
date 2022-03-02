using System.Collections.Generic;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

public interface IContent
{
    ContentId Id { get; }
    void Reload(Device device);    
}
