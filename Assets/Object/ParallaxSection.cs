using System;
using UnityEngine;

[Serializable]
public class ParallexSection
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    /// <summary>
    /// The layer index that this section belongs to.
    /// </summary>
    [SerializeField] private int layerIndex;
    public int LayerIndex => layerIndex;

    /// <summary>
    /// Tile ID that must be spawned before this section spawns.
    /// Set to -1 (default) for immediate spawn or if the dependency is invalid.
    /// When SpawnSection is called:
    /// - If dependencyTileId is -1: section spawns immediately
    /// - If dependencyTileId is valid (0 to tileCount-1): section waits until the matching tile loops
    /// - If dependencyTileId is invalid: section spawns immediately with a warning
    /// </summary>
    [SerializeField] private int dependencyTileId = -1;

    public int DependencyTileId => dependencyTileId;
}
