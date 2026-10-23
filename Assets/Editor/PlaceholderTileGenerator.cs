using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class PlaceholderTileGenerator
{
    const string Root = "Assets/Tiles";
    const int PixelsPerUnit = 16;   // à garder identique quand tu passeras à de vraies images

 struct Spec
    {
        public string name; 
        public Color color; 
        public bool solid; 
        public bool blocksPathing;

        public Spec(string n, Color c, bool solid = false, bool blocksPathing = false)
        { 
            name = n; 
            color = c; 
            this.solid = solid; 
            this.blocksPathing = blocksPathing; 
        }
    }

    static readonly Spec[] Specs =
    {
        new Spec("Floor",  new Color(0.45f, 0.40f, 0.35f)),
        new Spec("Wall",   new Color(0.20f, 0.20f, 0.25f), solid: true),
        new Spec("Rock",   new Color(0.35f, 0.35f, 0.38f), solid: true),
        new Spec("Mud",    new Color(0.35f, 0.25f, 0.15f)),
        new Spec("Lava",   new Color(0.90f, 0.35f, 0.10f), blocksPathing: true),
        new Spec("Spikes", new Color(0.70f, 0.70f, 0.75f)),
        new Spec("Pit",    new Color(0.05f, 0.05f, 0.08f), blocksPathing: true),
    };

    [MenuItem("Tools/Dungeon/Generate Placeholder Tiles")]
    static void Generate()
    {
        Directory.CreateDirectory($"{Root}/Tiles");
        Directory.CreateDirectory($"{Root}/Definitions");
        AssetDatabase.Refresh();

        Sprite white = GetOrCreateWhiteSprite();

        foreach (var s in Specs)
        {
            string tilePath = $"{Root}/Tiles/Tile_{s.name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = white;
                tile.color = s.color;
                tile.colliderType = s.solid ? Tile.ColliderType.Grid : Tile.ColliderType.None;
                AssetDatabase.CreateAsset(tile, tilePath);
            }

            string defPath = $"{Root}/Definitions/{s.name}.asset";
            if (AssetDatabase.LoadAssetAtPath<TileDefinition>(defPath) == null)
            {
                var def = ScriptableObject.CreateInstance<TileDefinition>();
                def.variants = new TileBase[] { tile };
                def.solid = s.solid;
                def.blocksPathing = s.blocksPathing;
                def.effects = new TileEffect[0];
                AssetDatabase.CreateAsset(def, defPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Tiles et TileDefinitions générées dans " + Root);
    }

    static Sprite GetOrCreateWhiteSprite()
    {
        string path = $"{Root}/White.png";
        if (!File.Exists(path))
        {
            var tex = new Texture2D(PixelsPerUnit, PixelsPerUnit);
            var px = new Color[PixelsPerUnit * PixelsPerUnit];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = PixelsPerUnit;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}