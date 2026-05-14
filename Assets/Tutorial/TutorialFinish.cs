using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // <-- IMPORTANTE: Necesario para controlar Images
using System.Collections;

public class CambioEscenaAutomatico : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject panelInteractuar;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textoPrompt;

    [Header("Simulación de Carga (Tipo Pastel)")]
    public Image imagenCargaRadial; // Arrastra aquí la imagen circular
    public float duracionFade = 0.5f;

    [Header("Idiomas (0 = ES, 1 = EN)")]
    public string[] mensajesLocalizados = {
        "Cambiando de escena en 3 segundos...",
        "Changing scene in 3 seconds..."
    };

    [Header("Configuración de Escena")]
    public string nombreEscenaCargar = "Tutorial";
    public float tiempoEspera = 3.0f; // Este tiempo controlará la velocidad del círculo

    private bool yaSeActivo = false;

    void Start()
    {
        if (panelInteractuar != null)
        {
            panelInteractuar.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha = 0;

            // Ponemos la imagen llena al inicio
            if (imagenCargaRadial != null) imagenCargaRadial.fillAmount = 1f;
        }
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
        if (panelInteractuar != null)
        {
            // 1. Idioma y setup inicial
            if (textoPrompt != null && LanguageManager.GetInstance() != null)
            {
                byte langIndex = LanguageManager.GetInstance().GetCurrentLanguageByte();
                if (langIndex < mensajesLocalizados.Length)
                    textoPrompt.text = mensajesLocalizados[langIndex];
            }

            // Asegurar que la imagen empiece llena
            if (imagenCargaRadial != null) imagenCargaRadial.fillAmount = 1f;

            panelInteractuar.SetActive(true);

            // 2. Fade In del panel entero
            if (canvasGroup != null)
            {
                float tiempoFade = 0;
                while (tiempoFade < duracionFade)
                {
                    tiempoFade += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0, 1, tiempoFade / duracionFade);
                    yield return null;
                }
                canvasGroup.alpha = 1;
            }
        }

        // --- 3. LÓGICA DE LA IMAGEN RADIAL (EFECTO PASTEL) ---
        if (imagenCargaRadial != null)
        {
            float tiempoTranscurrido = 0;

            // Mientras no se acabe el tiempo de espera...
            while (tiempoTranscurrido < tiempoEspera)
            {
                tiempoTranscurrido += Time.deltaTime;

                // Calculamos el porcentaje invertido (va de 1 a 0)
                // Usamos GetAxisRaw("desencriptado") matemático: 1 - (actual / total)
                float progresoInvertido = 1f - (tiempoTranscurrido / tiempoEspera);

                // Aplicamos el valor al fillAmount (0.0 a 1.0)
                imagenCargaRadial.fillAmount = progresoInvertido;

                yield return null; // Esperamos al siguiente frame
            }

            // Nos aseguramos de que quede totalmente vacía al final
            imagenCargaRadial.fillAmount = 0f;
        }
        else
        {
            // Si no hay imagen, igual esperamos el tiempo configurado
            yield return new WaitForSeconds(tiempoEspera);
        }

        // 4. Carga de escena
        if (!string.IsNullOrEmpty(nombreEscenaCargar))
        {
            SceneManager.LoadScene(nombreEscenaCargar);
        }
    }
}