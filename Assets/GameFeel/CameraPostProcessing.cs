using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class CameraPostProcessing : MonoBehaviour
{
   
    private static CameraPostProcessing instance;

    public static CameraPostProcessing GetInstance() 
    { 
        
        return instance;
    }



    private VolumeProfile profile;
    private UnityEngine.Rendering.Universal.Bloom bloomFx;

    private UnityEngine.Rendering.Universal.Vignette vignetteFx;


    public bool bloomFxOn;
    public bool bloomUp;
    float currentBloomIntensity;
    float aimBloomIntensity;
    float bloomSpeed;
    float normalBloomIntensity;

    public bool vignetteFxOn;
    public bool vignetteUp;
    float currentvignetteIntensity;
    float aimvignetteIntensity;
    float vignetteSpeed;
    float normalvignetteIntensity;

    void Awake()
    {

        instance = this;

        profile = GetComponent<Volume>().profile;

        profile.TryGet(out bloomFx);


        profile.TryGet(out vignetteFx);



    }

    public void SetBloomFx(float _intensityToReach = 10, int _framesToReach= 10)
    {
        bloomFxOn = true;
        bloomUp= true;
        currentBloomIntensity = bloomFx.intensity.value;
        aimBloomIntensity = _intensityToReach;  
        bloomSpeed = (aimBloomIntensity - currentBloomIntensity) / _framesToReach;


    }


    void BloomUpdate()
    {

        if (!bloomFxOn) return;

        if (bloomUp)
        {

            currentBloomIntensity += bloomSpeed;
            print("subo");

            if(currentBloomIntensity > aimBloomIntensity)
            {
                currentBloomIntensity = aimBloomIntensity;
                bloomUp = false;
                print("falso el bloom Up");

            }



        }
        else
        {
            print("bajo");
            currentBloomIntensity -= bloomSpeed*2;

            if (currentBloomIntensity < normalBloomIntensity)
            {
                currentBloomIntensity = normalBloomIntensity;
                bloomFxOn = false;

            }
        }
        bloomFx.intensity.Override(currentBloomIntensity);
    }


    public void SetVigneteFx(float _intensityToReach = 10, int _framesToReach = 10)
    {
        vignetteFxOn = true;
        vignetteUp = true;
        currentvignetteIntensity = vignetteFx.intensity.value;
        aimvignetteIntensity = _intensityToReach;
        vignetteSpeed = (aimvignetteIntensity - currentvignetteIntensity) / _framesToReach;


    }

    void vignetteUpdate()
    {

        if (!vignetteFxOn) return;

        if (vignetteUp)
        {

            currentvignetteIntensity += vignetteSpeed;
            print("subo");

            if (currentvignetteIntensity > aimvignetteIntensity)
            {
                currentvignetteIntensity = aimvignetteIntensity;
                vignetteUp = false;
            

            }



        }
        else
        {
        
            currentvignetteIntensity -= vignetteSpeed * 2;

            if (currentvignetteIntensity < normalvignetteIntensity)
            {
                currentvignetteIntensity = normalvignetteIntensity;
                bloomFxOn = false;

            }
        }
        vignetteFx.intensity.Override(currentvignetteIntensity);
    }

    private void FixedUpdate()
    {
        BloomUpdate();
        vignetteUpdate();
    }
}