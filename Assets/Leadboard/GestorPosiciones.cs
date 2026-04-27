using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    public Transform[] hitosDePista;
    public List<DatosCorredor> listaCorredores = new List<DatosCorredor>();

    [Header("Configuración de Carrera")]
    public int totalVueltas = 3;
    public GameObject panelFinCarrera;
    public TMPro.TextMeshProUGUI textoResultado;
    
    // --- NUEVA VARIABLE PARA VUELTAS ---
    [Tooltip("Arrastra aquí el texto del Canvas que mostrará las vueltas (ej: 1/3)")]
    public TMPro.TextMeshProUGUI textoVueltas;

    [Header("Nivel actual")]
    public int currentLevel = 1;
    private int corredoresFinalizados = 0;
    
    // Referencia interna para no buscar al jugador en cada frame
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

        // Guardamos la referencia del jugador para el HUD de vueltas
        datosJugador = listaCorredores.Find(c => c.transform.CompareTag("Player"));
        
        // Inicializamos el texto de vueltas
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

        if (datos.ultimoHito == hitosDePista.Length - 1 && indice == 0)
        {
            datos.vueltasDadas++;
            
            // Si es el jugador, actualizamos su contador visual en el HUD
            if (datos.transform.CompareTag("Player"))
            {
                ActualizarHUDVueltas();
            }

            if (datos.vueltasDadas >= totalVueltas)
            {
                FinalizarCarreraCorredor(datos);
                return;
            }
        }

        if (indice == (datos.ultimoHito + 1) % hitosDePista.Length)
        {
            datos.ultimoHito = indice;
        }
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

            if (panelFinCarrera != null) panelFinCarrera.SetActive(true);

            if (textoResultado != null && LanguageManager.GetInstance() != null)
            {
                if (LanguageManager.GetInstance().currentLanguage == LANGUAGES.SPANISH)
                    textoResultado.text = gano ? "Victoria" : "Posición: " + corredor.posicion + "°";
                else
                    textoResultado.text = gano ? "Victory" : "Position: " + corredor.posicion + "°";
            }

            if (gano) SlotSaveSystem.CompleteLevelInActiveSlot(currentLevel);

            StartCoroutine(EsperarYCambiarEscena(3f, gano));
        }
    }

    // --- FUNCIÓN PARA ACTUALIZAR EL TEXTO DE VUELTAS ---
    private void ActualizarHUDVueltas()
    {
        if (textoVueltas != null && datosJugador != null)
        {
            // Usamos Mathf.Min para que no muestre "4/3" si cruza la meta final
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

    void Update()
    {
        if (hitosDePista.Length == 0) return;

        foreach (var c in listaCorredores)
        {
            if (c.haTerminado) continue;
            int siguienteHito = (c.ultimoHito + 1) % hitosDePista.Length;
            c.distanciaAlSiguiente = Vector3.Distance(c.transform.position, hitosDePista[siguienteHito].position);
            c.progresoTotal = (c.vueltasDadas * 100000) + (c.ultimoHito * 1000) - c.distanciaAlSiguiente;
        }

        var ordenados = listaCorredores.OrderByDescending(c => c.progresoTotal).ToList();
        for (int i = 0; i < ordenados.Count; i++)
        {
            if (!ordenados[i].haTerminado) ordenados[i].posicion = i + 1;
        }
    }

    // --- FUNCIONES PARA OTROS SCRIPTS (MANTENIDAS VIVAS) ---
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
