namespace TowerDefense.Main.Map.Path
{
    public enum PathGraphBuildError
    {
        None = 0,
        StartIsEnd,
        NoRoute,
        TooManyRoutes,
        CellsOutsideRoutes,
        Cycle
    }
}
