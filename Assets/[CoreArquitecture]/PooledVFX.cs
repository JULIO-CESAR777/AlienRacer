using UnityEngine;

public class PooledVFX : MonoBehaviour
{
    private ObjectPool _pool;
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    public void Init(ObjectPool pool)
    {
        _pool = pool;
    }

    private void OnEnable()
    {
        if (_ps != null) _ps.Play();
    }

    private void Update()
    {
        // Return to pool once particles are done
        if (_ps != null && !_ps.IsAlive())
            _pool.Return(gameObject);
    }
}
