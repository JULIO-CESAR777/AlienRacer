using UnityEngine;
using System.Collections;

public class CoinObject : MonoBehaviour
{
    [Header("Efectos Visuales")]
    [Tooltip("Arrastra aquí el PREFAB de las partículas")]
    [SerializeField] private GameObject efectoParticulasPrefab;

    [Tooltip("OBJETO VACÍO:")]
    [SerializeField] private Transform puntoEmision;

    [Header("Configuración de Reaparición")]
    [SerializeField] private float tiempoReaparicion = 5f;

    private MeshRenderer _renderer;
    private Collider _collider;
    private bool _estaRecogida;

    private void Awake()
    {
        _renderer = GetComponentInChildren<MeshRenderer>();
        _collider = GetComponent<Collider>();
        if (puntoEmision == null)
        {
            puntoEmision = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_estaRecogida && other.CompareTag("Player"))
        {
            Recoger(other);
        }
    }

    private void Recoger(Collider player)
    {
        _estaRecogida = true;
        if (player.transform.root.TryGetComponent(out KartController kart))
        {
            kart.AddCoin();
        }

        if (efectoParticulasPrefab != null)
        {
            GameObject particulas = Instantiate(efectoParticulasPrefab, puntoEmision.position, Quaternion.identity);
            if (particulas.TryGetComponent(out ParticleSystem ps))
            {
                ps.Play();
            }
            Destroy(particulas, 2f);
        }

        // 3. DESACTIVAR VISUALES
        if (_renderer != null) _renderer.enabled = false;
        if (_collider != null) _collider.enabled = false;

        StartCoroutine(RutinaReaparicion());
    }

    private IEnumerator RutinaReaparicion()
    {
        yield return new WaitForSeconds(tiempoReaparicion);

        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
        _estaRecogida = false;
    }
}