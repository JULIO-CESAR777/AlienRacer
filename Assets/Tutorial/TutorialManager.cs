using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;
using System.Collections.Generic;

[Serializable]
public struct TutorialSprites
{
    public string nombreMecanica;

    public Sprite teclado;
    public Sprite xbox;

    [Header("Configuración de Monedas")]
    public int monedasObjetivo;

    [Header("Textos (0 = Español, 1 = Inglés)")]
    [TextArea] public string[] textoTeclado;
    [TextArea] public string[] textoXbox;
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

    [Header("Configuracion de Monedas UI")]
    [SerializeField] private TextMeshProUGUI coinGoalText;

    [SerializeField]
    private string[] mensajeMonedas =
    {
        "Recolecta {0}/{1} monedas",
        "Collect {0}/{1} coins"
    };

    [Header("Base de Datos de Imagenes")]
    public TutorialSprites[] listaMecanicas;

    // =========================================
    // NUEVO SISTEMA DE ESTADOS
    // =========================================

    private Dictionary<string, bool> tutorialStates =
        new Dictionary<string, bool>();

    // =========================================

    private TutorialSprites currentMecanica;
    private bool isTutorialActive = false;
    private bool isFading = false;
    private KartController playerKart;

    private void Awake()
    {
        instance = this;

        if (canvasGroup != null)
            canvasGroup.alpha = 0;

        panelTutorial.SetActive(false);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            playerKart = playerObj.GetComponent<KartController>();

        if (InputManager.instance != null)
        {
            InputManager.instance.OnChangeInputType += OnDeviceChanged;
        }

        if (LanguageManager.GetInstance() != null)
        {
            LanguageManager.GetInstance().OnLanguageChanged += OnLanguageChanged;
        }
    }

    private void Update()
    {
        if (isTutorialActive && currentMecanica.monedasObjetivo > 0)
        {
            ActualizarContadorMonedas();
        }
    }

    private void ActualizarContadorMonedas()
    {
        if (playerKart == null || coinGoalText == null)
            return;

        byte langIndex =
            LanguageManager.GetInstance().GetCurrentLanguageByte();

        coinGoalText.text = string.Format(
            mensajeMonedas[langIndex],
            playerKart.coins,
            currentMecanica.monedasObjetivo
        );

        if (playerKart.coins >= currentMecanica.monedasObjetivo)
            coinGoalText.color = Color.green;
        else
            coinGoalText.color = Color.white;
    }

    public void MostrarTutorial(string nombre)
    {
        if (isFading || isTutorialActive)
            return;

        foreach (var item in listaMecanicas)
        {
            if (item.nombreMecanica == nombre)
            {
                currentMecanica = item;

                if (coinGoalText != null)
                {
                    coinGoalText.gameObject.SetActive(
                        currentMecanica.monedasObjetivo > 0
                    );
                }

                UpdateUI(InputManager.instance.currentInputType);

                StartCoroutine(FadeTutorial(true));

                return;
            }
        }
    }

    public void OcultarTutorial()
    {
        if (isFading || !isTutorialActive)
            return;

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
        byte langIndex =
            LanguageManager.GetInstance().GetCurrentLanguageByte();

        switch (type)
        {
            case INPUT_TYPE.KEYBOARD:

                tutorialDisplayImage.sprite = currentMecanica.teclado;

                tutorialDisplayText.text =
                    GetLocalizedText(currentMecanica.textoTeclado, langIndex);

                break;

            case INPUT_TYPE.XBOX:

                tutorialDisplayImage.sprite = currentMecanica.xbox;

                tutorialDisplayText.text =
                    GetLocalizedText(currentMecanica.textoXbox, langIndex);

                break;

            default:

                tutorialDisplayImage.sprite = currentMecanica.teclado;

                tutorialDisplayText.text =
                    GetLocalizedText(currentMecanica.textoTeclado, langIndex);

                break;
        }

        if (tutorialDisplayImage.sprite != null)
            tutorialDisplayImage.SetNativeSize();
    }

    private string GetLocalizedText(string[] texts, byte index)
    {
        if (texts == null || texts.Length == 0)
            return "";

        if (index >= texts.Length)
            return texts[0];

        return texts[index];
    }

    private void OnDeviceChanged(INPUT_TYPE newType)
    {
        if (isTutorialActive)
            UpdateUI(newType);
    }

    private void OnLanguageChanged(LANGUAGES newLanguage)
    {
        if (isTutorialActive)
            UpdateUI(InputManager.instance.currentInputType);
    }

    public void CompleteTutorial(string tutorialID)
    {
        tutorialStates[tutorialID] = true;

        Debug.Log($"Tutorial completado: {tutorialID}");
    }

    public bool IsTutorialCompleted(string tutorialID)
    {
        if (tutorialStates.ContainsKey(tutorialID))
            return tutorialStates[tutorialID];

        return false;
    }

    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.OnChangeInputType -= OnDeviceChanged;
        }

        if (LanguageManager.GetInstance() != null)
        {
            LanguageManager.GetInstance().OnLanguageChanged -= OnLanguageChanged;
        }
    }
}