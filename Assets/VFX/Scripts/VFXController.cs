using UnityEngine;

public class VFXController : MonoBehaviour
{
    public static VFXController Instance { get; private set; }

    [Header("Collision VFX")]
    [SerializeField] private Transform _collisionVFXTransform = default;
    [SerializeField] private GameObject _collisionVFXPrefab = default;
    [SerializeField] private int _collisionPoolSize = 10;

    [Header("Boost VFX")]
    [SerializeField] private Transform _boostVFXTransform = default;
    [SerializeField] private GameObject _boostVFXPrefab = default;
    [SerializeField] private int _boostPoolSize = 5;

    [Header("Dead VFX")]
    [SerializeField] private Transform _deadVFXTransform = default;
    [SerializeField] private GameObject _deadVFXPrefab = default;
    [SerializeField] private int _deadPoolSize = 5;

    [Header("Victory VFX")]
    [SerializeField] private Transform _victoryVFXTransform = default;
    [SerializeField] private GameObject _victoryVFXPrefab = default;
    [SerializeField] private int _victoryPoolSize = 4;
    
    [Header("Spawn VFX")]
    [SerializeField] private Transform _spawnVFXTransform = default;
    [SerializeField] private GameObject _spawnVFXPrefab = default;
    [SerializeField] private int _spawnPoolSize = 5;

    private ObjectPool _collisionPool;
    private ObjectPool _boostPool;
    private ObjectPool _deadPool;
    private ObjectPool _victoryPool;
    private ObjectPool _spawnPool;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Initialize all pools at startup
        _collisionPool = CreatePool(_collisionVFXPrefab, _collisionVFXTransform, _collisionPoolSize);
        _boostPool     = CreatePool(_boostVFXPrefab,     _boostVFXTransform,     _boostPoolSize);
        _deadPool      = CreatePool(_deadVFXPrefab,      _deadVFXTransform,      _deadPoolSize);
        _victoryPool   = CreatePool(_victoryVFXPrefab,   _victoryVFXTransform,   _victoryPoolSize);
        _spawnPool     = CreatePool(_spawnVFXPrefab, _spawnVFXTransform, _spawnPoolSize);
    }

    public void SpawnCollisionVFX(Vector3 position) => SpawnFromPool(_collisionPool, position, "Collision");
    public void SpawnBoostVFX(Vector3 position)     => SpawnFromPool(_boostPool,     position, "Boost");
    public void SpawnDeadVFX(Vector3 position)      => SpawnFromPool(_deadPool,      position, "Dead");
    public void SpawnVictoryVFX(Vector3 position)   => SpawnFromPool(_victoryPool,   position, "Victory");
    public void SpawnSpawnVFX(Vector3 position)     => SpawnFromPool(_spawnPool,     position, "Spawn");

    private ObjectPool CreatePool(GameObject prefab, Transform parent, int size)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[VFXController] A prefab is not assigned — skipping pool creation.");
            return null;
        }
        return new ObjectPool(prefab, parent, size);
    }

    private void SpawnFromPool(ObjectPool pool, Vector3 position, string vfxName)
    {
        if (pool == null)
        {
            Debug.LogWarning($"[VFXController] {vfxName} pool is null — was the prefab assigned?");
            return;
        }

        GameObject obj = pool.Get(position);
        // Hook up auto-return if the prefab has PooledVFX on it
        if (obj.TryGetComponent(out PooledVFX vfx))
            vfx.Init(pool);
    }
}