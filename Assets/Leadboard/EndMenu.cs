using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndRaceMenuController : MonoBehaviour
{
    [Header("Menu Options")]
    [Tooltip("0 = Reintentar, 1 = Menú Principal")]
    public Selectable[] endMenuOptions;

    [Header("Configuración de Auto-Carga (Victoria)")]
    [SerializeField] private GameObject contenedorConteo; // El objeto que contiene la imagen de carga
    [SerializeField] private Image imagenCargaFilled;     // La imagen con Image Type = Filled
    [SerializeField] private float tiempoDeEspera = 5f;    // Cuánto tiempo tarda en cargar

    private int currentMenuIndex;
    private bool canMove;
    private InputManager input;
    private bool playerWon;
    private Coroutine winCoroutine;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (contenedorConteo != null) contenedorConteo.SetActive(false);
    }

    private void Start()
    {
        input = InputManager.GetInstance();
    }

    public void MostrarPanel(bool gano)
    {
        playerWon = gano;
        gameObject.SetActive(true);

        if (playerWon)
        {
            // Si ganó, iniciamos el conteo visual
            winCoroutine = StartCoroutine(RutinaConteoVictoria());
        }
        else
        {
            // Si perdió, nos aseguramos que el conteo esté apagado
            if (contenedorConteo != null) contenedorConteo.SetActive(false);
        }
    }

    private IEnumerator RutinaConteoVictoria()
    {
        if (contenedorConteo == null || imagenCargaFilled == null) yield break;

        contenedorConteo.SetActive(true);
        float timer = 0;

        while (timer < tiempoDeEspera)
        {
            timer += Time.deltaTime;
            imagenCargaFilled.fillAmount = timer / tiempoDeEspera;
            yield return null;
        }

        // Al terminar el tiempo, carga la siguiente escena
        CargarSiguienteNivel();
    }

    private void OnEnable()
    {
        if (input == null) input = InputManager.GetInstance();
        currentMenuIndex = 0;
        canMove = false;
        SelectCurrentOption();
    }

    private void Update()
    {
        if (input == null || endMenuOptions == null || endMenuOptions.Length == 0) return;

        HandleVerticalNavigation();
        HandleSubmit();
    }

    private void HandleVerticalNavigation()
    {
        float verticalStick = input.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        float verticalDpad = input.GetAXis(AXIS.VERTICAL_DPAD);
        float vertical = Mathf.Abs(verticalDpad) > Mathf.Abs(verticalStick) ? verticalDpad : verticalStick;

        if (vertical > 0.5f && canMove)
        {
            canMove = false;
            MoveSelection(-1);
            SelectCurrentOption();
        }
        else if (vertical < -0.5f && canMove)
        {
            canMove = false;
            MoveSelection(1);
            SelectCurrentOption();
        }
        else if (Mathf.Abs(vertical) < 0.3f)
        {
            canMove = true;
        }
    }

    private void HandleSubmit()
    {
        if (!input.IsButtonDown(BUTTONS.B)) return;
        if (!IsValidSelectableIndex(currentMenuIndex)) return;

        // Si el jugador presiona un botón manualmente, cancelamos el auto-conteo
        if (winCoroutine != null) StopCoroutine(winCoroutine);

        ExecuteAction();
    }

    public void ExecuteAction()
    {
        Time.timeScale = 1f;

        switch (currentMenuIndex)
        {
            case 0: // REINTENTAR
                ReintentarNivel();
                break;

            case 1: // SALIR AL MENÚ
                IrAlMenu();
                break;
        }
    }

    private void ReintentarNivel()
    {
        CargarEscenaSegura(SceneManager.GetActiveScene().buildIndex);
    }

    private void CargarSiguienteNivel()
    {
        CargarEscenaSegura(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void IrAlMenu()
    {
        if (UINavigationController.Instance != null)
        {
            UINavigationController.GetInstance().SetDestination(UINavigationController.UIDestination.MainMenu);
        }
        CargarEscenaSegura(0);
    }

    // --- LÓGICA DE NAVEGACIÓN (Mantenida) ---
    private void MoveSelection(int direction)
    {
        int attempts = 0;
        do
        {
            currentMenuIndex += direction;
            if (currentMenuIndex < 0) currentMenuIndex = endMenuOptions.Length - 1;
            else if (currentMenuIndex >= endMenuOptions.Length) currentMenuIndex = 0;
            attempts++;
            if (IsValidSelectableIndex(currentMenuIndex)) return;
        } while (attempts < endMenuOptions.Length);
    }

    private void SelectCurrentOption()
    {
        if (!IsValidSelectableIndex(currentMenuIndex)) currentMenuIndex = GetFirstValidSelectableIndex();
        if (currentMenuIndex != -1) endMenuOptions[currentMenuIndex].Select();
    }

    private bool IsValidSelectableIndex(int index) => (endMenuOptions != null && index >= 0 && index < endMenuOptions.Length && endMenuOptions[index] != null && endMenuOptions[index].interactable);

    private int GetFirstValidSelectableIndex()
    {
        for (int i = 0; i < endMenuOptions.Length; i++) if (IsValidSelectableIndex(i)) return i;
        return -1;
    }

    private void CargarEscenaSegura(int index)
    {
        try
        {
            if (SceneLoader.GetInstance() != null)
            {
                SceneLoader.GetInstance().LoadScene(index);
                return;
            }
        }
        catch { }
        SceneManager.LoadScene(index);
    }
}