using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    
    [Header("Camera Controller")]
    [SerializeField] private MenuCameraController cameraController;
    [SerializeField] private MenuAnimatorController playAnims;

    [Header("Sistema de guardado")]
    [SerializeField] private SaveSlotButtonHandler saveSlot;
    
    [Header("Sistema de idioma")]
    [SerializeField] private LanguageManager languageManager;
    
    private DisplaySettings displaySettings;
    
    [Header("Menu actual")]
    public Selectable[] mainMenu;
    public Selectable[] playMenu;
    public Selectable[] slotsMenu;
    public Selectable[] settingsMenu;

    private Selectable[] currentMenu;
    public int currentMenuIndex;
    public int currentHorizontal;
    public int menusIndex;

    private bool canMove;
    private bool canMoveHorizontally;
    
    
    private InputManager input;
    private float goUp;
    private float goDown;
    private float moveInput;
    

    private void Start()
    {
        displaySettings = DisplaySettings.GetInstance();
        input = InputManager.GetInstance();
        canMove = true;

        currentMenu = mainMenu;
        menusIndex = 0;
        currentMenuIndex = 0;

        SelectCurrentOption();
    }

    private void Update()
    {
        if (input == null || currentMenu == null || currentMenu.Length == 0) return;
        
        HandleVerticalNavigation();
        HandleHorizontalNavigation();
        HandleSubmit();
        
    }

    private void HandleVerticalNavigation()
    {
        // Sube
        if (input.GetAXis(AXIS.LEFT_STICK_VERTICAL) > 0 && canMove)
        {
            canMove = false;
            currentMenuIndex--;
            if (currentMenuIndex < 0)
            {
                currentMenuIndex = currentMenu.Length - 1;
            }
        }
        // Baja
        else if (input.GetAXis(AXIS.LEFT_STICK_VERTICAL) < 0 && canMove)
        {
            canMove = false;
            currentMenuIndex++;
            if (currentMenuIndex > currentMenu.Length - 1)
            {
                currentMenuIndex = 0;
            }
            
        }else if (input.GetAXis(AXIS.LEFT_STICK_VERTICAL) == 0)
        {
            canMove = true;
        }
        
        SelectCurrentOption();
    }
    
    private void HandleSubmit()
    {
        if (!input.IsButtonDown(BUTTONS.B)) return;

        Selectable current = currentMenu[currentMenuIndex];
        
        MenusActions(menusIndex);
    }

    private void HandleHorizontalNavigation()
    {
        float horizontal = input.GetAXis(AXIS.LEFT_STICK_HORIZONTAL);

        if (horizontal > 0 && canMoveHorizontally)
        {
            canMoveHorizontally = false;
            OnHorizontalInput(true);
        }
        else if (horizontal < 0 && canMoveHorizontally)
        {
            canMoveHorizontally = false;
            OnHorizontalInput(false);
        }
        else if (horizontal == 0)
        {
            canMoveHorizontally = true;
        }
    }
    
    private void OnHorizontalInput(bool right)
    {
        switch (currentMenuIndex)
        {
            case 0:
            {
                languageManager.ChangeLanguage(right);
                break;
            }

            case 1:
            {
                displaySettings.ChangeMode(right);
                break;
            }

            case 2:
            {
                Slider slider = currentMenu[currentMenuIndex].GetComponent<Slider>();
                if (slider != null)
                {
                    slider.value += right ? 0.1f : -0.1f;
                }
                break;
            }

            case 3:
            {
                Slider slider = currentMenu[currentMenuIndex].GetComponent<Slider>();
                if (slider != null)
                {
                    slider.value += right ? 0.1f : -0.1f;
                }
                break;
            }
        }
    }

    private void SelectCurrentOption()
    {
        if (currentMenu[currentMenuIndex] == null) return;
        currentMenu[currentMenuIndex].Select();
    }

    public void MenusActions(int index)
    {
        MenuInteractable(false);
        switch (index)
        {
            case 0:
            {
                MainMenuActions();
                break;
            }
            case 1:
            {
                PlayMenuActions();
                break;
            }
            case 2:
            {
                PlaySettinsActions();
                break;
            }
            case 3:
            {
                break;
            }
            case 4:
            {
                PlaySlotsActions();
                break;
            }
        }
        MenuInteractable(true);
        currentMenuIndex = 0;
        SelectCurrentOption();
    }

    public void MenuInteractable(bool flag)
    {
        for (int i = 0; i < currentMenu.Length; i++)
        {
            currentMenu[i].interactable = flag;
        }
    }

    private void MainMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
            {
                cameraController.GoToPlay();
                currentMenu = playMenu;
                menusIndex = 1;
                break;
            }
            case 1:
            {
                cameraController.GoToSettings();
                currentMenu = settingsMenu;
                menusIndex = 2;
                break;
            }
            case 2:
            {
                // Que te lleve a los controles
                break;
            }
            case 3:
            {
                Application.Quit();
                break;
            }
        }
    }

    private void PlayMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
            {
                // Se pone lo que pasaria con el tutorial
                break;
            }
            case 1:
            {
                playAnims.AbrirMenu();
                currentMenu = slotsMenu;
                menusIndex = 4;
                break;
            }
            case 2:
            {
                cameraController.GoToMenuFromPlay();
                currentMenu = mainMenu;
                menusIndex = 0;
                break;
            }
        }
            
    }

    private void PlaySlotsActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
            {
                saveSlot.SelectSlot(0);
                break;
            }
            case 1:
            {
                saveSlot.SelectSlot(1);
                break;
            }
            case 2:
            {
                saveSlot.SelectSlot(2);
                break;
            }
            case 3:
            {
                playAnims.CerrarMenu();
                currentMenu = playMenu;
                menusIndex = 1;
                break;
            }
        }
    }

    private void PlaySettinsActions()
    {
        switch (currentMenuIndex)
        {
            case 4:
            {
                cameraController.GoToMenuFromSettings();
                currentMenu = mainMenu;
                menusIndex = 0;
                break;
            }
        }
    }
    
}