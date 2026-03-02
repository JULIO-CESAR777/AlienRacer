using UnityEngine;

public class ParticipanteCarrera : MonoBehaviour
{
    public string nombreEntidad;
    public int posicionActual;

    [HideInInspector] public int ultimoHitoPasado = 0;
    [HideInInspector] public float distanciaAlSiguiente;

    public float ObtenerProgreso()
    {
        // Multiplicamos el hito por 10000 para que siempre sea más importante que la cercanía
        return (ultimoHitoPasado * 10000) - distanciaAlSiguiente;
    }

    private void Update()
    {
        // Calculamos la distancia al siguiente punto para desempatar
        if (GestorPosiciones.instancia.hitosDePista.Length > 0)
        {
            int indiceSiguiente = (ultimoHitoPasado + 1) % GestorPosiciones.instancia.hitosDePista.Length;
            distanciaAlSiguiente = Vector3.Distance(transform.position, GestorPosiciones.instancia.hitosDePista[indiceSiguiente].position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint")) 
        {
            int indice = other.GetComponent<InfoHito>().indiceHito;

            // Solo cuenta si es el siguiente hito en orden (evita saltarse curvas)
            int hitoEsperado = (ultimoHitoPasado + 1) % GestorPosiciones.instancia.hitosDePista.Length;
            if (indice == hitoEsperado)
            {
                ultimoHitoPasado = indice;
            }
        }
    }
}