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

    // --- CONDICIÓN DE VICTORIA ---
    public int vueltasDadas;
    public bool haTerminado;
}

public class GestorPosiciones : MonoBehaviour
{
    public static GestorPosiciones Instancia;

    public Transform[] hitosDePista;
    public List<DatosCorredor> listaCorredores = new List<DatosCorredor>();

    // --- VARIABLES DE CONFIGURACIÓN ---
    [Header("Configuración de Carrera")]
    public int totalVueltas = 3;
    public GameObject panelFinCarrera;
    public TMPro.TextMeshProUGUI textoResultado;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject); // Seguridad para que no haya dos gestores
    }

    void Start()
    {
        // Buscamos por Tag 
        ConfigurarCorredoresConTag("Player");
        ConfigurarCorredoresConTag("Bot");
    }

    void ConfigurarCorredoresConTag(string tag)
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objetos)
        {
            listaCorredores.Add(new DatosCorredor { transform = obj.transform, ultimoHito = 0 });
        }
    }

    // El hito llama a esta función
    public void RegistrarPasoPorHito(Transform corredor, int indice)
    {
        var datos = listaCorredores.Find(c => c.transform == corredor);

        if (datos == null || datos.haTerminado) return;

        // Lógica de vueltas...
        if (datos.ultimoHito == hitosDePista.Length - 1 && indice == 0)
        {
            datos.vueltasDadas++;
            // Log opcional para vueltas
            Debug.Log($"<color=yellow>¡{corredor.name} completó la vuelta {datos.vueltasDadas}!</color>");

            if (datos.vueltasDadas >= totalVueltas)
            {
                FinalizarCarreraCorredor(datos);
            }
        }

        if (indice == (datos.ultimoHito + 1) % hitosDePista.Length)
        {
            datos.ultimoHito = indice;
            Debug.Log($"Corredor: <b>{corredor.name}</b> paso por el <b>Hito {indice}</b>");
        }
    }
    // --- LÓGICA DE FINALIZACIÓN ---
    private void FinalizarCarreraCorredor(DatosCorredor corredor)
    {
        corredor.haTerminado = true;

        // Si es el jugador, mostramos la UI de resultados
        if (corredor.transform.CompareTag("Player"))
        {
            if (panelFinCarrera != null) panelFinCarrera.SetActive(true);
            if (textoResultado != null)
            {
                textoResultado.text = (corredor.posicion == 1) ? "¡VICTORIA!" : "Posición: " + corredor.posicion + "°";
            }
        }
    }

    void Update()
    {
        if (hitosDePista.Length == 0) return;

        // progreso de todos
        foreach (var c in listaCorredores)
        {
            // Si ya terminó, no seguimos recalculando su progreso para que mantenga su posición final
            if (c.haTerminado) continue;

            int siguienteHito = (c.ultimoHito + 1) % hitosDePista.Length;
            c.distanciaAlSiguiente = Vector3.Distance(c.transform.position, hitosDePista[siguienteHito].position);
            // vueltas para que tengan prioridad absoluta
            c.progresoTotal = (c.vueltasDadas * 100000) + (c.ultimoHito * 1000) - c.distanciaAlSiguiente;
        }

        // Ordenamos por progreso
        var ordenados = listaCorredores.OrderByDescending(c => c.progresoTotal).ToList();

        // Asignamos el número de posición
        for (int i = 0; i < ordenados.Count; i++)
        {
            ordenados[i].posicion = i + 1;
        }
    }

    // --- FUNCIONES PARA OTROS SCRIPTS ---
    public int ObtenerPosicionDe(Transform coche)
    {
        var datos = listaCorredores.Find(c => c.transform == coche);
        return datos != null ? datos.posicion : 0;
    }
    //función Gacha para calcular
    public int ObtenerTotalCorredores()
    {
        return listaCorredores.Count;
    }
}