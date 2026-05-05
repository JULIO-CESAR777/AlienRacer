using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; 

public class CambioEscenaAutomatico : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject panelInteractuar;
    public TextMeshProUGUI textoPrompt;
    public string mensajeCorta = "Cambiando de escena en 3 segundos...";

    [Header("Configuración de Escena")]
    public string nombreEscenaCargar;
    public float tiempoEspera = 3.0f;

    private bool yaSeActivo = false;

    void Start()
    {
        if (panelInteractuar != null)
            panelInteractuar.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !yaSeActivo)
        {
            yaSeActivo = true; 
            StartCoroutine(SecuenciaCambioEscena());
        }
    }

    IEnumerator SecuenciaCambioEscena()
    {
        if (panelInteractuar != null) panelInteractuar.SetActive(true);
        if (textoPrompt != null) textoPrompt.text = mensajeCorta;
        yield return new WaitForSeconds(tiempoEspera);
        if (!string.IsNullOrEmpty(nombreEscenaCargar))
        {
            SceneManager.LoadScene(nombreEscenaCargar);
        }
        else
        {
            Debug.LogError("Nombre de escena no asignado en el Inspector.");
        }
    }
}