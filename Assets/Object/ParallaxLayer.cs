using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParallaxLayer
{
    /// <summary>
    /// Every tile in this layer (in order).
    /// </summary>
    [SerializeField] private List<Transform> tiles = new List<Transform>();
    public List<Transform> Tiles => tiles;

    [SerializeField][Range(0f, 2f)] private float speedMultiplier = 0.5f;
    public float SpeedMultiplier => speedMultiplier;

    /// <summary>
    /// Whether to allow skipping the movement of this layer when it spawns.
    /// </summary>
    [SerializeField] private bool allowSkipMovementOnSpawn = true;
    public bool AllowSkipMovementOnSpawn => allowSkipMovementOnSpawn;

    private float tileWidth;
    public float TileWidth
    {
        get => tileWidth;
        set => tileWidth = value;
    }
}
