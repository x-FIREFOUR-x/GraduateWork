using System.Collections.Generic;

using UnityEngine;

using static TowerDefense.EditorTools.UI.SpriteRaster;


namespace TowerDefense.EditorTools.UI
{
    // The sprites the side menus share, the shop of the game and the menu of the map constructor: the dark column
    // with a golden edge, the frame of a chosen card with its diamond, the golden line under a name and the arched
    // window an icon shows through. Both builders bake them through here, so neither has to wait for the other
    public static class SideMenuSprites
    {
        public const string Folder = "Assets/Sprites/UI/SideMenu/";
        // How far the light round a chosen card reaches past it
        public const float GlowMargin = 6f;

        private const float pixels = CommonSprites.Pixels;

        private static readonly Color columnColor = Hex("141f17", 0.94f);
        private static readonly Color windowColor = Hex("0f1912");
        private static readonly Color gold = Hex("e2c172");
        private static readonly Color darkGold = Hex("8a6a2e");
        private static readonly Color goldEdge = Hex("5a3e12");
        private static readonly (float, Color)[] polishedGold =
        {
            (0f, Hex("fff2c0")), (0.35f, Hex("e6bf62")), (0.62f, Hex("9a6e22")), (1f, Hex("f0d488")),
        };


        // Dark, with a golden line down its right edge and a thin one inside it
        public static Sprite Column()
        {
            const float size = 64f;
            Rect all = new(0f, 0f, size, size);
            SpriteRaster raster = new(all, pixels);
            raster.Draw(_ => -1f, all, Solid(columnColor));
            raster.Draw(p => Mathf.Abs(p.x - (size - 0.75f)) - 0.75f, all, Solid(gold));
            raster.Draw(p => Mathf.Abs(p.x - (size - 4.2f)) - 0.4f, all, Solid(darkGold));
            return raster.Save(Folder + "Column.png", pixels * 100f, new Vector4(3f, 3f, 8f * pixels, 3f));
        }

        // The frame of a chosen card: a golden line with a thin one inside, and a soft golden light round it
        public static Sprite Selected()
        {
            Rect all = new(-GlowMargin, -GlowMargin, 64f + GlowMargin * 2f, 64f + GlowMargin * 2f);
            SpriteRaster raster = new(all, pixels);
            raster.Draw(p => CommonSprites.CardDistance(p, -GlowMargin), all, p =>
            {
                float d = CommonSprites.CardDistance(p, 0f);
                Color c = gold;
                c.a = d <= 0f ? 0f : 0.4f * Mathf.Pow(1f - Mathf.Clamp01(d / GlowMargin), 2f);
                return c;
            });
            raster.Draw(p => Mathf.Abs(CommonSprites.CardDistance(p, 1f)) - 1f, all, Solid(gold));
            raster.Draw(p => Mathf.Abs(CommonSprites.CardDistance(p, 3.5f)) - 0.3f, all, Solid(darkGold));
            float border = (10f + GlowMargin) * pixels;
            return raster.Save(Folder + "CardSelected.png", pixels * 100f, new Vector4(border, border, border, border));
        }

        public static Sprite Diamond()
        {
            SpriteRaster raster = new(new Rect(0f, 0f, 14f, 9f), pixels);
            List<Vector2> diamond = Points(7, 0.3f, 13.7f, 4.5f, 7, 8.7f, 0.3f, 4.5f);
            raster.Fill(diamond, Vertical(0f, 9f, polishedGold));
            raster.Stroke(diamond, 0.4f, Solid(goldEdge), true);
            return raster.Save(Folder + "SelectedDiamond.png", 100f);
        }

        // A golden line fading out at both ends
        public static Sprite NameLine()
        {
            Rect all = new(0f, 0f, 66f, 2f);
            SpriteRaster raster = new(all, pixels);
            raster.Draw(p => Mathf.Abs(p.y - 1f) - 0.5f, all, p =>
            {
                Color c = gold;
                c.a = Mathf.Pow(1f - Mathf.Abs(p.x - 33f) / 33f, 1.5f);
                return c;
            });
            return raster.Save(Folder + "NameLine.png", 100f);
        }

        public static Sprite WindowBack()
        {
            return Window("WindowBack", false);
        }

        public static Sprite WindowFrame()
        {
            return Window("WindowFrame", true);
        }

        // The arched window the icon shows through: round at the top, nearly square at the bottom
        private static float WindowDistance(Vector2 p)
        {
            const float radius = 45f;
            float arch = (p - new Vector2(radius, radius)).magnitude - radius;
            float box = RoundBoxDistance(p - new Vector2(radius, (radius + 84f) / 2f), new Vector2(radius, (84f - radius) / 2f), 0.5f, 0.5f, 4f, 4f);
            return Mathf.Min(arch, box);
        }

        private static Sprite Window(string name, bool frame)
        {
            Rect all = new(0f, 0f, 90f, 84f);
            SpriteRaster raster = new(all, pixels);
            if (frame)
                raster.Draw(p => Mathf.Abs(WindowDistance(p) + 0.6f) - 0.6f, all, Vertical(0f, 84f, polishedGold));
            else
                raster.Draw(WindowDistance, all, Solid(windowColor));
            return raster.Save(Folder + name + ".png", 100f);
        }
    }

}
