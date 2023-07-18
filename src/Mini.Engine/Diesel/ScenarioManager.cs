using Mini.Engine.Configuration;
using Mini.Engine.Diesel.Tracks;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel;

[Service]
public sealed class ScenarioManager
{

    public ScenarioManager()
    {
        // TODO: this should later load the grid from some preconfigured settings or something like that
        this.Grid = new TrackGrid(100, 100, STRAIGHT_LENGTH, STRAIGHT_LENGTH);
    }


    public TrackGrid Grid { get; }
}
