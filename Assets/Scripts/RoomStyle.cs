using UnityEngine;

[System.Serializable]
public class HazardRule
{
    public TileDefinition tile;
    [Range(0f, 0.5f)] public float coverage = 0.08f;   // part de l'intérieur de la salle
    public int clusterSize = 4;                        // taille des "taches"
    public int minLevel = 0;
    [Tooltip("Vide = tous les types de salle.")]
    public RoomType[] roomTypes;

    public bool AppliesTo(RoomType type, int level)
    {
        if (level < minLevel) return false;
        if (roomTypes == null || roomTypes.Length == 0) return true;
        return System.Array.IndexOf(roomTypes, type) >= 0;
    }
}

[System.Serializable]
public class PropRule
{
    public GameObject prefab;                 // coffre, levier, pot cassable...
    public Vector2Int countRange = new Vector2Int(0, 2);
}

[CreateAssetMenu(menuName = "Dungeon/Room Style")]
public class RoomStyle : ScriptableObject
{
    public TileDefinition floor;
    public TileDefinition wall;
    public Color doorOpenColor = new Color(0.25f, 0.2f, 0.15f);
    public Color doorLockedColor = new Color(0.55f, 0.15f, 0.15f);

    public HazardRule[] hazards;
    public PropRule[] props;

    [Range(0, 1)] public float templateChance = 0.5f;
    public RoomTemplate[] templates;
}

