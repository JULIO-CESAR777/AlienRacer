using UnityEngine;

public sealed class RotacionMoneda : MonoBehaviour
{
    [Header("Ajustes de Giro")]
    [Tooltip("Velocidad en grados por segundo sobre el eje Z")]
    public float velocidadGiro = 60f;

    void Update()
    {
        if (MainManager.GetInstance() != null)
        {
            // Si el estado actual NO es "Play", ignoramos el resto del código y no rota
            if (MainManager.GetInstance().gameState != GameState.Play)
            {
                return;
            }
        }
        transform.Rotate(Vector3.forward * (velocidadGiro * Time.deltaTime), Space.Self);
    }
}