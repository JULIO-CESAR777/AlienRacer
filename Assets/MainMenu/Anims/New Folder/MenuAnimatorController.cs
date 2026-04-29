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

    public void AbrirMenuBorrado()
    {
        animator.SetTrigger("AbrirBorrado");
    }

    public void CerrarMenuBorrado()
    {
        animator.SetTrigger("CerrarBorrado");
    }
    
    public void ChangeIsOnAnimFromMenu()
    {
        MainMenuController.GetInstance().ChangeIsOnAnim();
    }
}