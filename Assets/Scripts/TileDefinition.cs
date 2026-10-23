using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon/Tile", fileName = "Tile_")]
public class TileDefinition : ScriptableObject
{
    [Tooltip("Plusieurs variantes = tirage aléatoire. Accepte aussi des RuleTile (2D Tilemap Extras).")]
    public TileBase[] variants;

    [Tooltip("Bloque physiquement (mur, rocher) : va dans le Tilemap avec collider.")]
    public bool solid;

    [Tooltip("Infranchissable pour la génération (lave, trou) même si non solide.")]
    public bool blocksPathing;

    public TileEffect[] effects;

    public bool Obstructs => solid || blocksPathing;

    public TileBase Pick(System.Random rng) =>
        variants == null || variants.Length == 0 ? null : variants[rng.Next(variants.Length)];
}