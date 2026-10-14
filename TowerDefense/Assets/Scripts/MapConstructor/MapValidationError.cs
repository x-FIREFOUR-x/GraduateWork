namespace TowerDefense.MapConstructor
{
    public enum MapValidationError
    {
        None = 0,
        StartIsEnd,
        NoRoute,
        TooManyRoutes,
        CellsOutsideRoutes,
        Cycle,
        MissingStartBuilding,
        MissingEndBuilding,
        MissingStartAndEndBuildings,
        StartBuildingHasSeveralConnections,
        EndBuildingHasSeveralConnections,
        PathSquare,
    }
}
