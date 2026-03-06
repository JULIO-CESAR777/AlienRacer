using UnityEngine;
using System.Collections;

public class CoinObject : MonoBehaviour
{
    [Header("Configuración de Reaparición")]
    [Tooltip("Segundos que tarda en volver a aparecer")]
    [SerializeField] private float tiempoReaparicion = 5f;
    private MeshRenderer _renderer;
    private Collider _collider;
    private WaitForSeconds _espera;
    private bool _estaRecogida;
    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
        _espera = new WaitForSeconds(tiempoReaparicion);
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
        _renderer.enabled = false;
        _collider.enabled = false;

        StartCoroutine(RutinaReaparicion());
    }

    private IEnumerator RutinaReaparicion()
    {
        yield return _espera;
        _renderer.enabled = true;
        _collider.enabled = true;
        _estaRecogida = false;
    }
}