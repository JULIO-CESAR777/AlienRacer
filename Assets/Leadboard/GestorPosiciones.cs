using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GrupoHito
{
    public string nombreHito; 
    public List<Transform> variantesFisicas; //  arrastrar 1 o más transforms para el mismo hito
}

public class DatosCorredor
{
    public Transform transform;
    public int ultimoHito;
    public float distanciaAlSiguiente;
    public float progresoTotal;
    public int posicion;
    public int vueltasDadas;
    public bool haTerminado;
}

public class GestorPosiciones : MonoBehaviour
{
    public static GestorPosiciones Instancia;

    //Es un array de Grupos en lugar de solo Transforms
    public GrupoHito[] hitosDePista;

    public List<DatosCorredor> listaCorredores = new List<DatosCorredor>();

    [Header("Configuración de Carrera")]
    public int totalVueltas = 3;
    public GameObject panelFinCarrera;
    public TMPro.TextMeshProUGUI textoResultado;
    public TMPro.TextMeshProUGUI textoVueltas;

    [Header("Nivel actual")]
    public int currentLevel = 1;
    private int corredoresFinalizados = 0;
    private DatosCorredor datosJugador;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ConfigurarCorredoresConTag("Player");
        ConfigurarCorredoresConTag("Bot");
        datosJugador = listaCorredores.Find(c => c.transform.CompareTag("Player"));
        ActualizarHUDVueltas();
    }

    void ConfigurarCorredoresConTag(string tag)
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objetos)
        {
            listaCorredores.Add(new DatosCorredor { transform = obj.transform, ultimoHito = 0 });
        }
    }

    public void RegistrarPasoPorHito(Transform corredor, int indice)
    {
        var datos = listaCorredores.Find(c => c.transform == corredor);
        if (datos == null || datos.haTerminado) return;

        // Lógica de vuelta (Hito 0)
        if (datos.ultimoHito == hitosDePista.Length - 1 && indice == 0)
        {
            datos.vueltasDadas++;
            if (datos.transform.CompareTag("Player")) ActualizarHUDVueltas();

            if (datos.vueltasDadas >= totalVueltas)
            {
                FinalizarCarreraCorredor(datos);
                return;
            }
        }

        // Validación secuencial: solo acepta el siguiente hito
        if (indice == (datos.ultimoHito + 1) % hitosDePista.Length)
        {
            datos.ultimoHito = indice;
        }
    }

    void Update()
    {
        if (hitosDePista.Length == 0) return;

        foreach (var c in listaCorredores)
        {
            if (c.haTerminado) continue;

            int siguienteHitoIdx = (c.ultimoHito + 1) % hitosDePista.Length;

            // Calculamos la distancia al punto más cercano de las variantes del siguiente hito
            c.distanciaAlSiguiente = ObtenerDistanciaMasCercana(c.transform.position, siguienteHitoIdx);

            c.progresoTotal = (c.vueltasDadas * 100000) + (c.ultimoHito * 1000) - c.distanciaAlSiguiente;
        }

        var ordenados = listaCorredores.OrderByDescending(c => c.progresoTotal).ToList();
        for (int i = 0; i < ordenados.Count; i++)
        {
            if (!ordenados[i].haTerminado) ordenados[i].posicion = i + 1;
        }
    }

    // función para manejar atajos
    private float ObtenerDistanciaMasCercana(Vector3 posCorredor, int idxHito)
    {
        float minDist = float.MaxValue;
        foreach (Transform t in hitosDePista[idxHito].variantesFisicas)
        {
            float d = Vector3.Distance(posCorredor, t.position);
            if (d < minDist) minDist = d;
        }
        return minDist;
    }

    private void FinalizarCarreraCorredor(DatosCorredor corredor)
    {
        corredor.haTerminado = true;
        corredoresFinalizados++;
        corredor.posicion = corredoresFinalizados;
        corredor.progresoTotal = float.MaxValue - corredoresFinalizados;

        if (corredor.transform.CompareTag("Player"))
        {
            bool gano = corredor.posicion == 1;

            // --- 1. QUITAR EL CONTROL AL JUGADOR ---
            // Apagamos el script KartController para que ya no reciba input (acelerar, frenar, girar).
            // El coche seguirá moviéndose por inercia física, pero el jugador ya no interactúa con él.
            KartController controlJugador = corredor.transform.GetComponent<KartController>();
            if (controlJugador != null)
            {
                controlJugador.enabled = false;
            }

            // --- 2. MOSTRAR EL MENÚ ---
            if (panelFinCarrera != null)
            {
                EndRaceMenuController endMenu = panelFinCarrera.GetComponent<EndRaceMenuController>();
                if (endMenu != null)
                {
                    endMenu.MostrarPanel(gano);
                }
                else
                {
                    panelFinCarrera.SetActive(true);
                }
            }

            // --- 3. ACTUALIZAR TEXTOS ---
            if (textoResultado != null && LanguageManager.GetInstance() != null)
            {
                if (LanguageManager.GetInstance().currentLanguage == LANGUAGES.SPANISH)
                    textoResultado.text = gano ? "Victoria" : "Posición: " + corredor.posicion + "°";
                else
                    textoResultado.text = gano ? "Victory" : "Position: " + corredor.posicion + "°";
            }

            // --- 4. GUARDAR PROGRESO ---
            if (gano) SlotSaveSystem.CompleteLevelInActiveSlot(currentLevel);

            // ¡LISTO! Ya no hay Time.timeScale = 0f;
        }
    }

    private void ActualizarHUDVueltas()
    {
        if (textoVueltas != null && datosJugador != null)
        {
            int vueltaActual = Mathf.Min(datosJugador.vueltasDadas + 1, totalVueltas);
            textoVueltas.text = vueltaActual + " / " + totalVueltas;
        }
    }

    private IEnumerator EsperarYCambiarEscena(float delay, bool gano)
    {
        yield return new WaitForSeconds(delay);
        if (RaceResultSystem.Instance != null)
            RaceResultSystem.Instance.CargarResultado(gano);
    }

    // --- FUNCIONES PUBLICAS ---
    public int ObtenerPosicionDe(Transform coche)
    {
        var datos = listaCorredores.Find(c => c.transform == coche);
        return datos != null ? datos.posicion : 0;
    }

    public int ObtenerTotalCorredores()
    {
        return listaCorredores.Count;
    }
}

/* 
private IEnumerator CargarEscenaResultados(float delay)
{
    yield return new WaitForSeconds(delay);
    UIController.GetInstance()?.ToUIScene();
}
*/
