using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections; // Necesario para las Corrutinas
using TMPro;

[Serializable]
public struct TutorialSprites
{
    public string nombreMecanica;
    public Sprite teclado;
    public Sprite xbox;
    // public Sprite playstation;

    [TextArea] public string textoTeclado;
    [TextArea] public string textoXbox;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("Configuracion UI")]
    [SerializeField] private Image tutorialDisplayImage;
    [SerializeField] private TextMeshProUGUI tutorialDisplayText;
    [SerializeField] private GameObject panelTutorial;
    [SerializeField] private CanvasGroup canvasGroup; // Arrastra el CanvasGroup aquí
    [SerializeField] private float fadeSpeed = 2f;    // Velocidad del fade

    [Header("Base de Datos de Imagenes")]
    public TutorialSprites[] listaMecanicas;

    private TutorialSprites currentMecanica;
    private bool isTutorialActive = false;
    private bool isFading = false; // Bloqueo para evitar bugs

    private void Awake()
    {
        instance = this;
        // Inicializamos invisible y desactivado
        if (canvasGroup != null) canvasGroup.alpha = 0;
        panelTutorial.SetActive(false);
    }

    private void Start()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.OnChangeInputType += OnDeviceChanged;
        }
    }

    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.OnChangeInputType -= OnDeviceChanged;
        }
    }

    private void OnDeviceChanged(INPUT_TYPE newType)
    {
        if (isTutorialActive)
        {
            UpdateUI(newType);
        }
    }

    public void MostrarTutorial(string nombre)
    {
        // Si ya está haciendo un fade o ya está activo, ignoramos para no buguear
        if (isFading || isTutorialActive) return;

        foreach (var item in listaMecanicas)
        {
            if (item.nombreMecanica == nombre)
            {
                currentMecanica = item;
                UpdateUI(InputManager.instance.currentInputType);
                StartCoroutine(FadeTutorial(true)); // Inicia Fade In
                return;
            }
        }
    }

    public void OcultarTutorial()
    {
        // Si está haciendo fade o ya está oculto, no hacemos nada
        if (isFading || !isTutorialActive) return;

        StartCoroutine(FadeTutorial(false)); // Inicia Fade Out
    }

    private IEnumerator FadeTutorial(bool fadeIn)
    {
        isFading = true; // Bloqueamos nuevas acciones

        if (fadeIn)
        {
            isTutorialActive = true;
            panelTutorial.SetActive(true);
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
        else
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 0;
            panelTutorial.SetActive(false);
            isTutorialActive = false;
        }

        isFading = false; // Liberamos el bloqueo
    }

    private void UpdateUI(INPUT_TYPE type)
    {
        switch (type)
        {
            case INPUT_TYPE.KEYBOARD:
                tutorialDisplayImage.sprite = currentMecanica.teclado;
                tutorialDisplayText.text = currentMecanica.textoTeclado;
                break;
            case INPUT_TYPE.XBOX:
                tutorialDisplayImage.sprite = currentMecanica.xbox;
                tutorialDisplayText.text = currentMecanica.textoXbox;
                break;
            default:
                tutorialDisplayImage.sprite = currentMecanica.teclado;
                tutorialDisplayText.text = currentMecanica.textoTeclado;
                break;
        }
    }
}