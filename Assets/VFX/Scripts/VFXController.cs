using UnityEngine;

public class VFXController : MonoBehaviour
{
    public static VFXController Instance { get; private set; }

    [Header("Collision VFX")]
    [SerializeField] private Transform _collisionVFXTransform = default;
    [SerializeField] private GameObject _collisionVFXPrefab = default;
    [SerializeField] private int _collisionPoolSize = 10;

    [Header("Dead VFX")]
    [SerializeField] private Transform _deadVFXTransform = default;
    [SerializeField] private GameObject _deadVFXPrefab = default;
    [SerializeField] private int _deadPoolSize = 5;

    [Header("Victory VFX")]
    [SerializeField] private Transform _victoryVFXTransform = default;
    [SerializeField] private GameObject _victoryVFXPrefab = default;
    [SerializeField] private int _victoryPoolSize = 3;

    [Header("Boost VFX")]
    [SerializeField] private Transform _boostVFXTransform = default;
    [SerializeField] private GameObject _boostVFXPrefab = default;

    private ObjectPool _collisionPool;
    private ObjectPool _deadPool;
    private ObjectPool _victoryPool;
    private ParticleSystem _boostPS;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _collisionPool = CreatePool(_collisionVFXPrefab, _collisionVFXTransform, _collisionPoolSize);
        _deadPool      = CreatePool(_deadVFXPrefab,      _deadVFXTransform,      _deadPoolSize);
        _victoryPool   = CreatePool(_victoryVFXPrefab,   _victoryVFXTransform,   _victoryPoolSize);

        // Boost se maneja diferente — trail continuo
        if (_boostVFXPrefab != null)
        {
            GameObject boostObj = Instantiate(_boostVFXPrefab, _boostVFXTransform);
            boostObj.transform.localPosition = Vector3.zero;
            boostObj.transform.localRotation = Quaternion.identity;
            _boostPS = boostObj.GetComponent<ParticleSystem>();
            _boostPS.Stop();
        }
        else
        {
            Debug.LogWarning("[VFXController] Boost VFX prefab is not assigned!");
        }
    }

    public static VFXController GetInstance()
    {
        if (Instance == null)
            Debug.LogError("[VFXController] Es null. Asegúrate de que exista en la escena.");
        return Instance;
    }

    // One-shot VFX
    public void SpawnCollisionVFX(Vector3 position) => SpawnFromPool(_collisionPool, position, "Collision");
    public void SpawnDeadVFX(Vector3 position)      => SpawnFromPool(_deadPool,      position, "Dead");
    public void SpawnVictoryVFX(Vector3 position)   => SpawnFromPool(_victoryPool,   position, "Victory");

    // Boost VFX — trail continuo
    public void StartBoostVFX()
    {
        if (_boostPS != null) _boostPS.Play();
    }

    public void StopBoostVFX()
    {
        if (_boostPS != null) _boostPS.Stop();
    }

    private ObjectPool CreatePool(GameObject prefab, Transform parent, int size)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[VFXController] Un prefab no está asignado — saltando creación del pool.");
            return null;
        }
        return new ObjectPool(prefab, parent, size);
    }

    private void SpawnFromPool(ObjectPool pool, Vector3 position, string vfxName)
    {
        if (pool == null)
        {
            Debug.LogWarning($"[VFXController] {vfxName} pool es null — ¿está asignado el prefab?");
            return;
        }

        GameObject obj = pool.Get(position);
        if (obj.TryGetComponent(out PooledVFX vfx))
            vfx.Init(pool);
    }
}