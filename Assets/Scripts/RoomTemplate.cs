using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [System.Serializable] public struct Entry { public char symbol; public TileDefinition tile; }

    public Vector2Int size = new Vector2Int(15, 9);
    public Entry[] legend;
    [TextArea(8, 20)] public string ascii;   // ligne du haut = haut de la salle ; '.' = sol par défaut

    public void ApplyTo(TileDefinition[,] t, TileDefinition floor)
    {
        var lines = ascii.Replace("\r", "").Split('\n');
        int w = size.x, h = size.y;
        for (int row = 0; row < lines.Length && row < h; row++)
            for (int col = 0; col < lines[row].Length && col < w; col++)
            {
                int gx = col, gy = h - 1 - row;
                if (gx <= 0 || gy <= 0 || gx >= w - 1 || gy >= h - 1) continue; // périmètre = murs
                foreach (var e in legend)
                    if (e.symbol == lines[row][col] && e.tile != null) t[gx, gy] = e.tile;
            }
    }
}