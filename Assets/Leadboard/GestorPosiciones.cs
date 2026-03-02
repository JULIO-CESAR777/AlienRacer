using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GestorPosiciones : MonoBehaviour
{
    public static GestorPosiciones instancia;
    public Transform[] hitosDePista; // Arrastra aquí los objetos en orden
    public List<ParticipanteCarrera> participantes = new List<ParticipanteCarrera>();

    void Awake() => instancia = this;

    void Start()
    {
        // Buscamos automáticamente a todos por sus etiquetas
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");

        foreach (GameObject j in jugadores) participantes.Add(j.GetComponent<ParticipanteCarrera>());
        foreach (GameObject b in bots) participantes.Add(b.GetComponent<ParticipanteCarrera>());
    }

    void Update()
    {
        // Ordenamos la lista según el progreso calculado
        var listaOrdenada = participantes.OrderByDescending(p => p.ObtenerProgreso()).ToList();

        for (int i = 0; i < listaOrdenada.Count; i++)
        {
            listaOrdenada[i].posicionActual = i + 1;
        }
    }
}