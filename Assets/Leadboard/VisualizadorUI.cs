using UnityEngine;
using TMPro;

public class VisualizadorPosicionUI : MonoBehaviour
{
    public ParticipanteCarrera participanteASeguir;
    public TextMeshProUGUI textoPosicion;

    void Update()
    {
        if (participanteASeguir != null)
        {
            textoPosicion.text = participanteASeguir.posicionActual + "°";
        }
    }
}