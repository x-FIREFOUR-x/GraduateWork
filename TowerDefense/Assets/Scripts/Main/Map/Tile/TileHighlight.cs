using UnityEngine;


namespace TowerDefense.Main.Map.Tile
{
    public static class TileHighlight
    {
        private const float strength = 0.4f;

        public static void Apply(Renderer render, Color color)
        {
            Light(render, Color.white, color);
        }

        public static void Replace(Renderer render, Color color)
        {
            Light(render, color, color);
        }

        public static void Clear(Renderer render)
        {
            Material material = render.material;

            material.color = Color.white;
            material.SetColor("_EmissionColor", Color.black);
            material.DisableKeyword("_EMISSION");
        }


        private static void Light(Renderer render, Color albedo, Color emission)
        {
            Material material = render.material;

            material.color = albedo;
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.SetColor("_EmissionColor", emission * strength);
        }
    }

}
