using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Floor Theme")]
public class FloorTheme : ScriptableObject
{
    [System.Serializable] public struct Override { public RoomType type; public RoomStyle style; }

    public RoomStyle defaultStyle;
    [Tooltip("Si rempli, les salles Normal tirent un style au hasard dans cette liste.")]
    public RoomStyle[] normalStyles;
    public Override[] overrides;

    public RoomStyle GetStyle(RoomType t, System.Random rng)
    {
        foreach (var o in overrides)
            if (o.type == t && o.style != null) return o.style;

        if (t == RoomType.Normal && normalStyles != null && normalStyles.Length > 0)
            return normalStyles[rng.Next(normalStyles.Length)];

        return defaultStyle;
    }
}