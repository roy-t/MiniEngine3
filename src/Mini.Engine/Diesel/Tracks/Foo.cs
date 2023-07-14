namespace Mini.Engine.Diesel.Tracks;

/**
 * We start with a curve, a curve defines where a train can drive
 * Curves can overlap (like a crossing) but only connected cerves can be traversed
 * A TrackPiece is a visual represtation of 1 or more curves in a tile
 * - A crossing
 * - A switch
 * - A straight piece
 * - A bend
 * 
 * Players lay curves, for that we need to know where each curve is and how they are connected
 * Once the curve has been placed we need to update the TrackPiece instancing
 * 
 * Note that curves can go up and down (like on a hill) but can't ever go over each other
 * 
 **/

internal class Foo
{
}
