using TowerDefense.Main.Map.Path;


namespace TowerDefense.MapConstructor.UI
{
    public static class UncorrectMapDescriptions
    {
        public static string GetMissingBuildings(bool hasStartBuilding, bool hasEndBuilding)
        {
            if (!hasStartBuilding && !hasEndBuilding)
                return "Place the start and the end buildings on the map.";

            return !hasStartBuilding ? "Place the start building on the map." : "Place the end building on the map.";
        }

        public static string GetPathGraphBuildErrorDescription(PathGraphBuildError error)
        {
            return error switch
            {
                PathGraphBuildError.StartIsEnd => "The start and the end buildings stand on the same tile.",
                PathGraphBuildError.NoRoute => "The path tiles do not connect the start building with the end building.",
                PathGraphBuildError.TooManyRoutes => "The path has too many forks. Remove some branches.",
                PathGraphBuildError.CellsOutsideRoutes => "Some path tiles do not lead from the start to the end. Remove dead ends and path tiles that are not connected to the path.",
                PathGraphBuildError.Cycle => "Some path tiles can be passed only by walking back towards the start. Remove them or connect them so that enemies move forward.",
                _ => "The path is built incorrectly."
            };
        }
    }

}
