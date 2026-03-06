using UnityEngine;

public class AnimacionLlantas : MonoBehaviour
{
    public Transform[] llantasDelanteras;
    public Transform[] llantasTraseras;

    public float multiplicadorRodada = 200f;
    public float anguloMaximoDireccion = 35f;
    public float sensibilidadDireccion = 0.5f;
    public float suavidadDireccion = 15f;

    private Vector3 posicionAnterior;
    private float rotacionYAnterior;
    private float rotacionXActual = 0f;
    private float anguloDireccionActual = 0f;

    void Start()
    {
        posicionAnterior = transform.position;
        rotacionYAnterior = transform.eulerAngles.y;
    }

    void Update()
    {
        Vector3 desplazamiento = transform.position - posicionAnterior;
        float distancia = desplazamiento.magnitude;
        float direccionMovimiento = Vector3.Dot(transform.forward, desplazamiento.normalized) >= 0 ? 1f : -1f;

        rotacionXActual += distancia * multiplicadorRodada * direccionMovimiento;

        float velocidadAngularY = Mathf.DeltaAngle(rotacionYAnterior, transform.eulerAngles.y) / Time.deltaTime;
        float anguloObjetivo = Mathf.Clamp(velocidadAngularY * sensibilidadDireccion, -anguloMaximoDireccion, anguloMaximoDireccion);

        anguloDireccionActual = Mathf.Lerp(anguloDireccionActual, anguloObjetivo, suavidadDireccion * Time.deltaTime);

        foreach (Transform llanta in llantasDelanteras)
        {
            if (llanta != null)
            {
                llanta.localRotation = Quaternion.Euler(rotacionXActual, anguloDireccionActual, 0f);
            }
        }

        foreach (Transform llanta in llantasTraseras)
        {
            if (llanta != null)
            {
                llanta.localRotation = Quaternion.Euler(rotacionXActual, 0f, 0f);
            }
        }

        posicionAnterior = transform.position;
        rotacionYAnterior = transform.eulerAngles.y;
    }
}