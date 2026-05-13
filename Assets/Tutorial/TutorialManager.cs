using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

[Serializable]
public struct TutorialSprites
{
    public string nombreMecanica;
    public Sprite teclado;
    public Sprite xbox;
    // public Sprite playstation;

    [Header("Textos (0 = Español, 1 = Inglés)")]
    [TextArea] public string[] textoTeclado; // <-- Cambiado a arreglo
    [TextArea] public string[] textoXbox;    // <-- Cambiado a arreglo
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("Configuracion UI")]
    [SerializeField] private Image tutorialDisplayImage;
    [SerializeField] private TextMeshProUGUI tutorialDisplayText;
    [SerializeField] private GameObject panelTutorial;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("Base de Datos de Imagenes")]
    public TutorialSprites[] listaMecanicas;

    private TutorialSprites currentMecanica;
    private bool isTutorialActive = false;
    private bool isFading = false;

    private void Awake()
    {
        instance = this;
        if (canvasGroup != null) canvasGroup.alpha = 0;
        panelTutorial.SetActive(false);
    }

    private void Start()
    {
        // Suscripción al input
        if (InputManager.instance != null)
        {
            InputManager.instance.OnChangeInputType += OnDeviceChanged;
        }

        // Suscripción al cambio de idioma
        if (LanguageManager.GetInstance() != null)
        {
            LanguageManager.GetInstance().OnLanguageChanged += OnLanguageChanged;
        }
    }

    private void OnDestroy()
    {
        // Desuscripción al input
        if (InputManager.instance != null)
        {
            InputManager.instance.OnChangeInputType -= OnDeviceChanged;
        }

        // Desuscripción al cambio de idioma
        if (LanguageManager.GetInstance() != null)
        {
            LanguageManager.GetInstance().OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void OnDeviceChanged(INPUT_TYPE newType)
    {
        if (isTutorialActive)
        {
            UpdateUI(newType);
        }
    }

    // Nuevo método para escuchar el evento de idioma
    private void OnLanguageChanged(LANGUAGES newLanguage)
    {
        if (isTutorialActive)
        {
            // Forzamos la actualización de la UI con el input actual para reflejar el nuevo idioma
            UpdateUI(InputManager.instance.currentInputType);
        }
    }

    public void MostrarTutorial(string nombre)
    {
        if (isFading || isTutorialActive) return;

        foreach (var item in listaMecanicas)
        {
            if (item.nombreMecanica == nombre)
            {
                currentMecanica = item;
                UpdateUI(InputManager.instance.currentInputType);
                StartCoroutine(FadeTutorial(true));
                return;
            }
        }
    }

    public void OcultarTutorial()
    {
        if (isFading || !isTutorialActive) return;
        StartCoroutine(FadeTutorial(false));
    }

    private IEnumerator FadeTutorial(bool fadeIn)
    {
        isFading = true;

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

        isFading = false;
    }

    private void UpdateUI(INPUT_TYPE type)
    {
        // Obtenemos el índice del idioma actual (0 = Español, 1 = Inglés)
        byte langIndex = LanguageManager.GetInstance().GetCurrentLanguageByte();

        switch (type)
        {
            case INPUT_TYPE.KEYBOARD:
                tutorialDisplayImage.sprite = currentMecanica.teclado;
                tutorialDisplayText.text = GetLocalizedText(currentMecanica.textoTeclado, langIndex);
                break;
            case INPUT_TYPE.XBOX:
                tutorialDisplayImage.sprite = currentMecanica.xbox;
                tutorialDisplayText.text = GetLocalizedText(currentMecanica.textoXbox, langIndex);
                break;
            default:
                tutorialDisplayImage.sprite = currentMecanica.teclado;
                tutorialDisplayText.text = GetLocalizedText(currentMecanica.textoTeclado, langIndex);
                break;
        }

        if (tutorialDisplayImage.sprite != null)
        {
            tutorialDisplayImage.SetNativeSize();
        }
    }

    // Método de seguridad para evitar errores 
    private string GetLocalizedText(string[] texts, byte index)
    {
        if (texts == null || texts.Length == 0) return "";
        if (index >= texts.Length) return texts[0]; // Retorna el primer idioma si hay un desajuste de índices
        return texts[index];
    }
}