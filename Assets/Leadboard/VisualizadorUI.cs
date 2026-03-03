using UnityEngine;
using TMPro;

public class VisualizadorPosicionUI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform cocheJugador;
    public TextMeshProUGUI textoPosicion;

    void Update()
    {
        if (cocheJugador != null && textoPosicion != null)
        {
            // Le pedimos al Singleton la posición del transform del jugador
            int puesto = GestorPosiciones.Instancia.ObtenerPosicionDe(cocheJugador);

            // Mostramos 1°, 2°, etc. (Si es 0 es que aún no se registra)
            textoPosicion.text = puesto > 0 ? puesto + "°" : "--";
        }
    }
}