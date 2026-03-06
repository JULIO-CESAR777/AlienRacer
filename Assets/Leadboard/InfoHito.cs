using UnityEngine;

public class InfoHito : MonoBehaviour
{
    [Tooltip("El orden de este hito en la pista (0, 1, 2...)")]
    public int indiceHito;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bot"))
        {
            GestorPosiciones.Instancia.RegistrarPasoPorHito(other.transform, indiceHito);
        }
    }
}