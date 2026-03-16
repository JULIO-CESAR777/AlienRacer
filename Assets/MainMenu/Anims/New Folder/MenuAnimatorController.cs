using UnityEngine;

public class MenuAnimatorController : MonoBehaviour
{
    public Animator animator;

    public void AbrirMenu()
    {
        animator.SetTrigger("Abrir");
    }

    public void CerrarMenu()
    {
        animator.SetTrigger("Cerrar");
    }
}