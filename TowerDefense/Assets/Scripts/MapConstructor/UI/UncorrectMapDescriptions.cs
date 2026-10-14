namespace TowerDefense.MapConstructor.UI
{
    public static class UncorrectMapDescriptions
    {
        public static string GetMapValidationErrorDescription(MapValidationError error)
        {
            return error switch
            {
                MapValidationError.StartIsEnd => "The start and the end buildings stand on the same tile.",
                MapValidationError.NoRoute => "The path tiles do not connect the start building with the end building.",
                MapValidationError.TooManyRoutes => "The path has too many forks. Remove some branches.",
                MapValidationError.CellsOutsideRoutes => "Some path tiles do not lead from the start to the end. Remove dead ends and path tiles that are not connected to the path.",
                MapValidationError.Cycle => "Some path tiles can be passed only by walking back towards the start. Remove them or connect them so that enemies move forward.",
                MapValidationError.MissingStartBuilding => "Place the start building on the map.",
                MapValidationError.MissingEndBuilding => "Place the end building on the map.",
                MapValidationError.MissingStartAndEndBuildings => "Place the start and the end buildings on the map.",
                MapValidationError.StartBuildingHasSeveralConnections => "The start building can have connection with only one path tile.",
                MapValidationError.EndBuildingHasSeveralConnections => "The end building can have connection with only one path tile.",
                MapValidationError.PathSquare => "The path must be one tile wide. Path tiles can not form a 2x2 square.",
                _ => "The path is built incorrectly."
            };
        }
    }

}
