using System;
using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void DontAllowInput()
    {
        MenuNavigation.GetInstance().DoingAnAnimation();
    }

    public void AllowInput()
    {
        MenuNavigation.GetInstance().NotDoingAnAnimation();
    }


    public void GoToPlay()
    {
        anim.SetTrigger("Play");
    }

    public void GoToMenuFromPlay()
    {
        anim.SetTrigger("BackFromPlay");
    }

    public void GoToSettings()
    {
        anim.SetTrigger("Settings");
    }

    public void GoToMenuFromSettings()
    {
        anim.SetTrigger("BackFromSettings");
    }

    public void GoToControls()
    {
        anim.SetTrigger("Controls");
    }
}
