using UnityEngine;

using static TowerDefense.EditorTools.UI.SpriteRaster;


namespace TowerDefense.EditorTools.UI
{
    // The sprites all of the UI shares: the dark card with a thin golden frame, the golden pill of the buttons and the
    // round golden token. Each builder that needs them bakes them through here, and a bake that changes nothing leaves
    // the files alone, so no builder has to wait for another
    public static class CommonSprites
    {
        public const string Folder = "Assets/Sprites/UI/";
        public const float Pixels = 3f;

        private static readonly Color cardColor = Hex("1c2a1f");
        private static readonly Color pillColor = Hex("16201a");
        private static readonly Color gold = Hex("e2c172");
        private static readonly Color darkGold = Hex("8a6a2e");
        private static readonly (float, Color)[] tokenFace = { (0f, Hex("3a4a34")), (1f, Hex("141f17")) };
        private static readonly (float, Color)[] polishedGold =
        {
            (0f, Hex("fff2c0")), (0.35f, Hex("e6bf62")), (0.62f, Hex("9a6e22")), (1f, Hex("f0d488")),
        };


        // The card is 64 units square, rounded at the top left and the bottom right; inset brings it in by that much
        public static float CardDistance(Vector2 p, float inset)
        {
            return RoundBoxDistance(p - new Vector2(32f, 32f), Vector2.one * (32f - inset),
                Mathf.Max(7f - inset, 0.5f), Mathf.Max(2f - inset, 0.5f), Mathf.Max(7f - inset, 0.5f), Mathf.Max(2f - inset, 0.5f));
        }

        public static Sprite Card()
        {
            Rect all = new(0f, 0f, 64f, 64f);
            SpriteRaster raster = new(all, Pixels);
            raster.Draw(p => CardDistance(p, 0f), all, Solid(cardColor));
            raster.Draw(p => Mathf.Abs(CardDistance(p, 0.6f)) - 0.6f, all, Solid(darkGold));
            float border = 10f * Pixels;
            return raster.Save(Folder + "Card.png", Pixels * 100f, new Vector4(border, border, border, border));
        }

        public static Sprite Pill()
        {
            Rect all = new(0f, 0f, 40f, 30f);
            SpriteRaster raster = new(all, Pixels);
            float Shape(Vector2 p) => RoundBoxDistance(p - new Vector2(20f, 15f), new Vector2(20f, 15f), 15f, 15f, 15f, 15f);
            raster.Draw(Shape, all, Solid(pillColor));
            raster.Draw(p => Mathf.Abs(Shape(p) + 0.5f) - 0.5f, all, Solid(darkGold));
            float border = 15f * Pixels;
            return raster.Save(Folder + "Pill.png", Pixels * 100f, new Vector4(border, border, border, border));
        }

        // The pill of a button that has to stand out among the others: a broad polished golden frame with a thin line
        // inside it
        public static Sprite PillHighlighted()
        {
            Rect all = new(0f, 0f, 40f, 30f);
            SpriteRaster raster = new(all, Pixels);
            float Shape(Vector2 p) => RoundBoxDistance(p - new Vector2(20f, 15f), new Vector2(20f, 15f), 15f, 15f, 15f, 15f);
            raster.Draw(Shape, all, Solid(pillColor));
            raster.Draw(p => Mathf.Abs(Shape(p) + 1.25f) - 1.25f, all, Vertical(0f, 30f, polishedGold));
            raster.Draw(p => Mathf.Abs(Shape(p) + 3.4f) - 0.35f, all, Solid(darkGold));
            float border = 15f * Pixels;
            return raster.Save(Folder + "PillHighlighted.png", Pixels * 100f, new Vector4(border, border, border, border));
        }

        public static Sprite Token()
        {
            Rect all = new(0f, 0f, 20f, 20f);
            SpriteRaster raster = new(all, Pixels * 2f);
            Vector2 centre = new(10f, 10f);
            raster.Draw(p => (p - centre).magnitude - 10f, all, Radial(all, new Vector2(0.35f, 0.3f), 0.8f, tokenFace));
            raster.Draw(p => Mathf.Abs((p - centre).magnitude - 9.4f) - 0.6f, all, Solid(gold));
            return raster.Save(Folder + "Token.png", 100f);
        }
    }

}
