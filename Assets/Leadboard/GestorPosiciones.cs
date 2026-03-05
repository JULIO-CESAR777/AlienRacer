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
}

public class GestorPosiciones : MonoBehaviour
{
    
    public static GestorPosiciones Instancia;

    public Transform[] hitosDePista;
    public List<DatosCorredor> listaCorredores = new List<DatosCorredor>();

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject); // Seguridad para que no haya dos gestores
    }

    void Start()
    {
        // Buscamos a todos por Tag 
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
        if (datos != null)
        {
            if (indice == (datos.ultimoHito + 1) % hitosDePista.Length)
            {
                datos.ultimoHito = indice;
            }
        }
    }

    void Update()
    {
        if (hitosDePista.Length == 0) return;

        // 1. Calculamos progreso de todos
        foreach (var c in listaCorredores)
        {
            int siguienteHito = (c.ultimoHito + 1) % hitosDePista.Length;
            c.distanciaAlSiguiente = Vector3.Distance(c.transform.position, hitosDePista[siguienteHito].position);
            c.progresoTotal = (c.ultimoHito * 10000) - c.distanciaAlSiguiente;
        }

        // 2. Ordenamos por progreso
        var ordenados = listaCorredores.OrderByDescending(c => c.progresoTotal).ToList();

        // 3. Asignamos el número de posición
        for (int i = 0; i < ordenados.Count; i++)
        {
            ordenados[i].posicion = i + 1;
        }
    }

    // --- FUNCIONES PARA OTROS SCRIPTS ---

    // Esta función la usa la UI y el Gacha para saber el puesto
    public int ObtenerPosicionDe(Transform coche)
    {
        var datos = listaCorredores.Find(c => c.transform == coche);
        return datos != null ? datos.posicion : 0;
    }

    // Esta función la necesita el Gacha para calcular los pesos (Weights)
    public int ObtenerTotalCorredores()
    {
        return listaCorredores.Count;
    }
}