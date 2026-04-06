using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceResultSystem : MonoBehaviour
{
    public static RaceResultSystem Instance;

    [Header("Configuración de Escenas")]
    [Tooltip("Nombre de la escena del siguiente nivel.")]
    public string escenaSiguienteNivel;
    [Tooltip("Nombre de la escena actual para reintentar.")]
    public string escenaReintentar;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CargarResultado(bool gano)
    {
        if (gano)
        {
            if (!string.IsNullOrEmpty(escenaSiguienteNivel))
                SceneManager.LoadScene(escenaSiguienteNivel);
            else
                Debug.LogError("No has asignado 'Escena Siguiente Nivel' en el RaceResultSystem.");
        }
        else
        {
            if (!string.IsNullOrEmpty(escenaReintentar))
                SceneManager.LoadScene(escenaReintentar);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}