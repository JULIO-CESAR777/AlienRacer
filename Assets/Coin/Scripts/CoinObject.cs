using UnityEngine;
using System.Collections;

public class CoinObject : MonoBehaviour
{
    [Header("Efectos Visuales")]
    [Tooltip("Arrastra aquí el PREFAB de las partículas o el objeto hijo actual")]
    [SerializeField] private GameObject efectoParticulasPrefab;

    [Header("Configuración de Reaparición")]
    [SerializeField] private float tiempoReaparicion = 5f;

    private MeshRenderer _renderer;
    private Collider _collider;
    private bool _estaRecogida;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
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

        // 1. Lógica del juego
        if (player.transform.root.TryGetComponent(out KartController kart))
        {
            kart.AddCoin();
        }
        if (efectoParticulasPrefab != null)
        {
            // Creamos una copia de las partículas en el mundo
            GameObject particulas = Instantiate(efectoParticulasPrefab, transform.position, transform.rotation);

            // Si el objeto tiene un ParticleSystem, le damos Play
            if (particulas.TryGetComponent(out ParticleSystem ps))
            {
                ps.Play();
            }

            Destroy(particulas, 2f);
        }

        // 3. DESACTIVAR VISUALES (La moneda "desaparece" pero el script sigue vivo)
        _renderer.enabled = false;
        _collider.enabled = false;

        StartCoroutine(RutinaReaparicion());
    }

    private IEnumerator RutinaReaparicion()
    {
        yield return new WaitForSeconds(tiempoReaparicion);

        _renderer.enabled = true;
        _collider.enabled = true;
        _estaRecogida = false;
    }
}