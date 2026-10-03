using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ParallaxBackground : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Settings")]
    /// <summary>
    /// The list of parallax layers.
    /// </summary>
    [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();
    /// <summary>
    /// The list of parallax sections that can be spawned on layers.
    /// </summary>
    [SerializeField] private List<ParallexSection> sections = new List<ParallexSection>();
    [SerializeField] private bool loopInfinite = true;
    [SerializeField] private bool alignTilesAtStart = true;
    [SerializeField] private bool autoScroll;
    [SerializeField] private Vector2 autoScrollDirection = Vector2.left;
    [SerializeField] private float autoScrollSpeed = 2f;

    /// <summary>
    /// The previous position of the target, used to calculate movement delta for parallax effect.
    /// </summary>
    private Vector3 previousTargetPosition;
    /// <summary>
    /// A dictionary to keep track of active spawned sections per layer (only one section per layer is active at a time).
    /// </summary>
    private Dictionary<int, GameObject> activeSpawnedSections = new Dictionary<int, GameObject>();
    private Dictionary<int, float> activeSectionWidths = new Dictionary<int, float>();

    /// <summary>
    /// A dictionary to keep track of pending section spawns that are waiting for a specific tile
    /// to be spawned before they can spawn.
    /// </summary>
    private Dictionary<int, int> pendingSectionSpawns = new Dictionary<int, int>();
    private Dictionary<int, int> lastSpawnedTileIdPerLayer = new Dictionary<int, int>();

    /// <summary>
    /// A set to keep track of sections that have just spawned, used to skip movement on the first frame if enabled.
    /// </summary>
    private HashSet<int> sectionsJustSpawned = new HashSet<int>();

    private void Awake()
    {
        if (target == null && Camera.main != null)
        {
            target = Camera.main.transform;
        }

        previousTargetPosition = target != null ? target.position : Vector3.zero;
        InitializeLayers();

        // For testing purposes, spawn sections at intervals
        InvokeRepeating(nameof(preTestSpawnSection), 5f, 5f);
        InvokeRepeating(nameof(preTestSpawnSection2), 5f, 5f);
        InvokeRepeating(nameof(preTestSpawnSection3), 5f, 5f);
    }

    #region Tests

    private void preTestSpawnSection()
    {
        SpawnSection(0);
    }

    private void preTestSpawnSection2()
    {
        SpawnSection(1);
    }

    private void preTestSpawnSection3()
    {
        SpawnSection(2);
    }

    #endregion

    private void Update()
    {
        // Calculate the movement delta based on the target's movement (camera) and auto-scroll
        Vector3 delta = Vector3.zero;

        if (target != null)
        {
            delta += target.position - previousTargetPosition;
        }

        if (autoScroll && autoScrollDirection.sqrMagnitude > 0f)
        {
            Vector2 scroll = autoScrollDirection.normalized * autoScrollSpeed * Time.deltaTime;
            delta += new Vector3(scroll.x, scroll.y, 0f);
        }

        for (int i = 0; i < layers.Count; i++)
        {
            ParallaxLayer layer = layers[i];
            MoveLayer(layer, delta);

            if (loopInfinite)
            {
                LoopLayer(layer, i);
            }

            if (activeSpawnedSections.ContainsKey(i) && activeSpawnedSections[i] != null)
            {
                MoveSectionWithLayer(layer, delta, i);
            }
        }

        // Check sections waiting to exit
        UpdateSectionsWaitingForExit();

        if (target != null)
        {
            previousTargetPosition = target.position;
        }
    }

    /// <summary>
    /// Initializes the layers by calculating their tile widths and aligning tiles.
    /// </summary>
    private void InitializeLayers()
    {
        foreach (ParallaxLayer layer in layers)
        {
            if (layer == null)
            {
                continue;
            }

            layer.TileWidth = GetTileWidth(layer);

            if (alignTilesAtStart)
            {
                AlignTiles(layer);
            }
        }
    }

    /// <summary>
    /// Aligns the tiles of a given layer based on the target's position and the camera's width.
    /// </summary>
    private void AlignTiles(ParallaxLayer layer)
    {
        if (layer == null || layer.Tiles.Count < 2 || layer.TileWidth <= 0f)
        {
            return;
        }

        Transform firstTile = layer.Tiles[0];
        if (firstTile == null)
        {
            return;
        }

        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;

        float fullCycleWidth = layer.TileWidth * layer.Tiles.Count;

        Vector3 startPosition = firstTile.position;

        if (autoScrollDirection.x < 0f)
        {
            startPosition.x = target.position.x - cameraWidth * 0.5f - fullCycleWidth * 0.5f;
        }
        else if (autoScrollDirection.x > 0f)
        {
            startPosition.x = target.position.x + cameraWidth * 0.5f - fullCycleWidth * 0.5f;
        }

        firstTile.position = startPosition;

        for (int i = 1; i < layer.Tiles.Count; i++)
        {
            Transform tile = layer.Tiles[i];
            if (tile == null)
            {
                continue;
            }

            tile.position = startPosition + new Vector3(layer.TileWidth * i, 0f, 0f);
        }
    }

    /// <summary>
    /// Moves a given layer based on the movement delta and the layer's speed multiplier.
    /// </summary>
    private void MoveLayer(ParallaxLayer layer, Vector3 delta)
    {
        if (layer == null)
        {
            return;
        }

        Vector3 parallaxOffset = new Vector3(delta.x * layer.SpeedMultiplier, delta.y * layer.SpeedMultiplier, 0f);

        for (int i = 0; i < layer.Tiles.Count; i++)
        {
            if (layer.Tiles[i] != null)
            {
                layer.Tiles[i].position += parallaxOffset;
            }
        }
    }

    /// <summary>
    /// Loops the tiles of a given layer to create an infinite scrolling effect based on the target's position and the camera's width.
    /// </summary>
    private void LoopLayer(ParallaxLayer layer, int layerIndex)
    {
        if (layer == null || layer.Tiles.Count < 2 || layer.TileWidth <= 0f || target == null)
        {
            return;
        }

        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
        float despawnDistance = cameraWidth * 0.5f + layer.TileWidth;

        for (int i = 0; i < layer.Tiles.Count; i++)
        {
            Transform tile = layer.Tiles[i];
            if (tile == null)
            {
                continue;
            }

            float distance = target.position.x - tile.position.x;
            float fullWidth = 0f;

            if (autoScrollDirection.x < 0f && distance > despawnDistance)
            {
                if (activeSpawnedSections.ContainsKey(layerIndex) && activeSpawnedSections[layerIndex] != null)
                {
                    float sectionX = activeSpawnedSections[layerIndex].transform.position.x;
                    if (tile.position.x >= sectionX)
                    {
                        continue;
                    }
                }

                fullWidth = layer.TileWidth * layer.Tiles.Count;
                if (activeSpawnedSections.ContainsKey(layerIndex) && activeSectionWidths.ContainsKey(layerIndex))
                {
                    fullWidth += activeSectionWidths[layerIndex];
                }

                tile.position += new Vector3(fullWidth, 0f, 0f);
                lastSpawnedTileIdPerLayer[layerIndex] = i;
                CheckPendingSectionDependencies(layerIndex);

            }
            else if (autoScrollDirection.x > 0f && distance < -despawnDistance)
            {
                if (activeSpawnedSections.ContainsKey(layerIndex) && activeSpawnedSections[layerIndex] != null)
                {
                    float sectionX = activeSpawnedSections[layerIndex].transform.position.x;
                    if (tile.position.x <= sectionX)
                    {
                        continue;
                    }
                }

                fullWidth = layer.TileWidth * layer.Tiles.Count;
                if (activeSpawnedSections.ContainsKey(layerIndex) && activeSectionWidths.ContainsKey(layerIndex))
                {
                    fullWidth += activeSectionWidths[layerIndex];
                }

                tile.position -= new Vector3(fullWidth, 0f, 0f);
                lastSpawnedTileIdPerLayer[layerIndex] = i;
                CheckPendingSectionDependencies(layerIndex);
            }
        }
    }

    /// <summary>
    /// Gets the width of a tile in a given layer by checking the first tile's SpriteRenderer bounds.
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    private float GetTileWidth(ParallaxLayer layer)
    {
        Transform source = null;

        if (layer.Tiles.Count > 0)
        {
            source = layer.Tiles[0];
        }

        if (source != null && source.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            return spriteRenderer.bounds.size.x;
        }

        return 0f;
    }

    /// <summary>
    /// Checks if any pending section can be spawned based on the last spawned tile ID for the given layer.
    /// </summary>
    /// <param name="layerIndex"></param>
    private void CheckPendingSectionDependencies(int layerIndex)
    {
        List<int> toRemove = new List<int>();
        List<int> keys = new List<int>(pendingSectionSpawns.Keys);

        foreach (int sectionId in keys)
        {
            if (!pendingSectionSpawns.ContainsKey(sectionId))
            {
                continue;
            }

            int dependencyTileId = pendingSectionSpawns[sectionId];

            if (sectionId >= sections.Count)
            {
                toRemove.Add(sectionId);
                continue;
            }

            ParallexSection section = sections[sectionId];
            if (section.LayerIndex != layerIndex)
            {
                continue;
            }

            if (dependencyTileId < 0 || dependencyTileId >= layers[layerIndex].Tiles.Count)
            {
                Debug.LogWarning($"Section {sectionId} has invalid dependency tile ID {dependencyTileId}. Spawning anyway.");
                toRemove.Add(sectionId);
                DoSpawnSection(sectionId);
                continue;
            }

            if (lastSpawnedTileIdPerLayer.ContainsKey(layerIndex) && lastSpawnedTileIdPerLayer[layerIndex] == dependencyTileId)
            {
                toRemove.Add(sectionId);
                DoSpawnSection(sectionId);
            }
        }

        foreach (int sectionId in toRemove)
        {
            pendingSectionSpawns.Remove(sectionId);
        }
    }

    /// <summary>
    /// Spawns a section by ID. If the section has a dependency tile ID, it waits for that tile.
    /// If a section is already active on the target layer, the new spawn is rejected.
    /// </summary>
    public void SpawnSection(int sectionId)
    {
        if (sectionId < 0 || sectionId >= sections.Count)
        {
            Debug.LogWarning($"Section ID {sectionId} is out of range.");
            return;
        }

        ParallexSection section = sections[sectionId];

        if (section.Prefab == null)
        {
            Debug.LogWarning($"Section {sectionId} prefab is not assigned.");
            return;
        }

        if (section.LayerIndex < 0 || section.LayerIndex >= layers.Count)
        {
            Debug.LogWarning($"Section {sectionId} layer index {section.LayerIndex} is out of range.");
            return;
        }

        if (activeSpawnedSections.ContainsKey(section.LayerIndex))
        {
            Debug.LogWarning($"Section already active on layer {section.LayerIndex}. Ignoring new spawn.");
            return;
        }

        if (section.DependencyTileId >= 0)
        {
            pendingSectionSpawns[sectionId] = section.DependencyTileId;
            return;
        }

        DoSpawnSection(sectionId);
    }

    /// <summary>
    /// Spawns a section by ID.
    /// </summary>
    private void DoSpawnSection(int sectionId)
    {
        ParallexSection section = sections[sectionId];
        ParallaxLayer targetLayer = layers[section.LayerIndex];

        float sectionWidth = 0;
        SpriteRenderer tempRenderer = section.Prefab.GetComponent<SpriteRenderer>();
        if (tempRenderer != null)
        {
            sectionWidth = tempRenderer.bounds.size.x;
        }

        float spawnX;

        if (autoScrollDirection.x < 0f)
        {
            float rightmostTileX = float.NegativeInfinity;
            foreach (Transform tile in targetLayer.Tiles)
            {
                if (tile != null && tile.position.x > rightmostTileX)
                {
                    rightmostTileX = tile.position.x;
                }
            }
            spawnX = rightmostTileX + targetLayer.TileWidth * 0.5f + sectionWidth * 0.5f;
        }
        else
        {
            float leftmostTileX = float.PositiveInfinity;
            foreach (Transform tile in targetLayer.Tiles)
            {
                if (tile != null && tile.position.x < leftmostTileX)
                {
                    leftmostTileX = tile.position.x;
                }
            }
            spawnX = leftmostTileX - targetLayer.TileWidth * 0.5f - sectionWidth * 0.5f;
        }

        Vector3 spawnPosition = new Vector3(spawnX, transform.position.y, transform.position.z);

        if (targetLayer.Tiles.Count > 0)
        {
            spawnPosition.y = targetLayer.Tiles[0].position.y;
        }

        GameObject spawnedSection = Instantiate(section.Prefab, spawnPosition, Quaternion.identity, transform);
        activeSpawnedSections[section.LayerIndex] = spawnedSection;
        activeSectionWidths[section.LayerIndex] = sectionWidth;

        SpriteRenderer spriteRenderer = spawnedSection.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            activeSectionWidths[section.LayerIndex] = spriteRenderer.bounds.size.x;
        }

        sectionsJustSpawned.Add(section.LayerIndex);
        pendingSectionSpawns.Remove(sectionId);
    }

    /// <summary>
    /// Moves the spawned section along with its layer based on the movement delta and the layer's speed multiplier.
    /// </summary>
    private void MoveSectionWithLayer(ParallaxLayer layer, Vector3 delta, int layerIndex)
    {
        if (!activeSpawnedSections.ContainsKey(layerIndex) || activeSpawnedSections[layerIndex] == null)
        {
            return;
        }

        // Skip movement on the frame the section spawned to avoid offset (if enabled)
        if (layer.AllowSkipMovementOnSpawn && sectionsJustSpawned.Contains(layerIndex))
        {
            sectionsJustSpawned.Remove(layerIndex);
            return;
        }

        Vector3 parallaxOffset = new Vector3(delta.x * layer.SpeedMultiplier, delta.y * layer.SpeedMultiplier, 0f);
        activeSpawnedSections[layerIndex].transform.position += parallaxOffset;
    }


    /// <summary>
    /// Updates sections waiting to exit and destroys them when they're out of view.
    /// </summary>
    private void UpdateSectionsWaitingForExit()
    {
        if (target == null)
        {
            return;
        }

        foreach (int layerIndex in new List<int>(activeSpawnedSections.Keys))
        {

            float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float sectionWidth = activeSectionWidths.ContainsKey(layerIndex) ? activeSectionWidths[layerIndex] : 10f;
            float despawnThreshold = cameraHalfWidth + sectionWidth * 0.5f;
            bool shouldDespawn = false;

            if (autoScrollDirection.x < 0f)
            {
                shouldDespawn = target.position.x - activeSpawnedSections[layerIndex].transform.position.x >= despawnThreshold;
            }
            else if (autoScrollDirection.x > 0f)
            {
                shouldDespawn = activeSpawnedSections[layerIndex].transform.position.x - target.position.x >= despawnThreshold;
            }

            if (shouldDespawn)
            {
                Destroy(activeSpawnedSections[layerIndex]);
                activeSpawnedSections.Remove(layerIndex);
                activeSectionWidths.Remove(layerIndex);
            }
        }
    }
}