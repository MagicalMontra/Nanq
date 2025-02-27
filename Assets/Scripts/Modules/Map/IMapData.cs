namespace SETHD.FantasySnake.Map
{
    public interface IMapData
    {
        float BlockUnit { get; }
        byte SizeX { get; }
        byte SizeY { get; }
        
        BlockData[] BlockDatas { get; }
    }
}