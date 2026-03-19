using UnityEngine;

public class LightProximityControl : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("componente Light.")]
    [SerializeField] private GameObject objetoLuz;
    private int cochesEnRango = 0;

    private void Start()
    {
        if (objetoLuz != null)
        {
            objetoLuz.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player") || other.transform.root.CompareTag("Bot"))
        {
            cochesEnRango++;
            ActualizarEstadoLuz();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Al salir del rango
        if (other.transform.root.CompareTag("Player") || other.transform.root.CompareTag("Bot"))
        {
            cochesEnRango--;
            if (cochesEnRango < 0) cochesEnRango = 0;
            ActualizarEstadoLuz();
        }
    }

    private void ActualizarEstadoLuz()
    {
        if (objetoLuz == null) return;
        objetoLuz.SetActive(cochesEnRango > 0);
    }
}