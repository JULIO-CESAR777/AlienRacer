using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // <-- Importante para cambiar el texto del botón

public class EndRaceMenuController : MonoBehaviour
{
    [Header("Menu Options")]
    [Tooltip("Arrastra aquí los botones. 0 = Inteligente (Reintentar/Sig Nivel), 1 = Menú Principal")]
    public Selectable[] endMenuOptions;

    [Header("Configuración del Botón Inteligente")]
    public TextMeshProUGUI textoBotonPrincipal; // Arrastra aquí el Text (TMP) del primer botón
    public string textoReintentar = "REINTENTAR";
    public string textoSiguienteNivel = "SIGUIENTE NIVEL";

    private int currentMenuIndex;
    private bool canMove;
    private InputManager input;
    private bool playerWon; // Guarda si ganamos o no

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        input = InputManager.GetInstance();
    }

    // --- NUEVO: LLAMA A ESTO DESDE TU GESTOR AL TERMINAR LA CARRERA ---
    public void MostrarPanel(bool gano)
    {
        playerWon = gano;

        // Cambiamos el texto antes de prender el panel
        if (textoBotonPrincipal != null)
        {
            textoBotonPrincipal.text = playerWon ? textoSiguienteNivel : textoReintentar;
        }

        gameObject.SetActive(true);
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

        float vertical = Mathf.Abs(verticalDpad) > Mathf.Abs(verticalStick)
            ? verticalDpad
            : verticalStick;

        // --- ARREGLO DE RESPONSIVIDAD AQUÍ ---
        if (vertical > 0.5f && canMove)
        {
            canMove = false;
            MoveSelection(-1);
            SelectCurrentOption();
        }
        else if (vertical < -0.5f && canMove) // <-- Cambiado de 0 a -0.5f
        {
            canMove = false;
            MoveSelection(1);
            SelectCurrentOption();
        }
        else if (Mathf.Abs(vertical) < 0.3f) // <-- Cambiado de == 0 a < 0.3f
        {
            canMove = true;
        }
    }

    private void HandleSubmit()
    {
        if (!input.IsButtonDown(BUTTONS.B)) return; // Usa B o A según ocupes
        if (!IsValidSelectableIndex(currentMenuIndex)) return;

        ExecuteAction();
    }

    private void MoveSelection(int direction)
    {
        if (endMenuOptions == null || endMenuOptions.Length == 0) return;

        int attempts = 0;

        do
        {
            currentMenuIndex += direction;

            if (currentMenuIndex < 0)
                currentMenuIndex = endMenuOptions.Length - 1;
            else if (currentMenuIndex >= endMenuOptions.Length)
                currentMenuIndex = 0;

            attempts++;

            if (IsValidSelectableIndex(currentMenuIndex))
                return;

        } while (attempts < endMenuOptions.Length);
    }

    private void SelectCurrentOption()
    {
        if (!IsValidSelectableIndex(currentMenuIndex))
        {
            currentMenuIndex = GetFirstValidSelectableIndex();
        }

        if (currentMenuIndex == -1) return;

        endMenuOptions[currentMenuIndex].Select();
    }

    private bool IsValidSelectableIndex(int index)
    {
        if (endMenuOptions == null) return false;
        if (index < 0 || index >= endMenuOptions.Length) return false;
        if (endMenuOptions[index] == null) return false;

        return endMenuOptions[index].interactable;
    }

    private int GetFirstValidSelectableIndex()
    {
        if (endMenuOptions == null) return -1;

        for (int i = 0; i < endMenuOptions.Length; i++)
        {
            if (IsValidSelectableIndex(i))
                return i;
        }

        return -1;
    }

    public void ExecuteAction()
    {
        Time.timeScale = 1f;
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        switch (currentMenuIndex)
        {
            case 0: // BOTÓN INTELIGENTE (REINTENTAR / SIGUIENTE NIVEL)
                if (playerWon)
                {
                    CargarEscenaSegura(currentScene + 1); // Carga el siguiente nivel
                }
                else
                {
                    CargarEscenaSegura(currentScene); // Reintenta el actual
                }
                break;

            case 1: // SALIR AL MENÚ
                if (UINavigationController.Instance != null)
                {
                    UINavigationController.GetInstance().SetDestination(UINavigationController.UIDestination.MainMenu);
                }

                CargarEscenaSegura(0);
                break;
        }
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
        catch
        {
            // Plan B silencioso
        }

        Debug.LogWarning("Usando SceneManager nativo.");
        SceneManager.LoadScene(index);
    }
}