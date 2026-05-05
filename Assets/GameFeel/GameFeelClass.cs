using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class GameFeelClass : MonoBehaviour
{

    public Animator anim;
    public int moveDistance = 1;


    //Shakeeeeeee shake shake shake

    public Vector3 startPosition;
    public float _distance;

    float shakeDuration;
    float shakePower;
    bool shaking;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            print("boton");
            transform.Translate(transform.forward * moveDistance);
            anim.SetTrigger("Hit");
            CameraPostProcessing.GetInstance().SetBloomFx(15, 15);
            CameraPostProcessing.GetInstance().SetVigneteFx(0.8f, 15);



        }
    }

    public void SetShake(float _time, float _power)
    {
        startPosition = transform.localPosition;
        shakeDuration = _time;
        shakePower = _power;
        shaking = true;
        

    }

    public void SetShake( float _time, float _power, Vector3 _position, float _maxDistance)
    {
        startPosition = transform.localPosition;
        shakeDuration = _time;
        shakePower = _power;
    }

    void Shaking()
    {


    }

}
