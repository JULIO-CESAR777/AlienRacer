using UnityEngine;

public sealed class RotacionMoneda : MonoBehaviour
{
    [Header("Ajustes de Giro")]
    [Tooltip("Velocidad en grados por segundo sobre el eje Z")]
    public float velocidadGiro = 60f;

    void Update()
    {
 
        transform.Rotate(Vector3.forward * (velocidadGiro * Time.deltaTime), Space.Self);
    }
}