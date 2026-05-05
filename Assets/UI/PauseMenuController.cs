using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menu actual")]
    public Selectable[] pauseMenu;
    public Selectable[] settingsMenu;
    
    
    private Selectable[] currentMenu;
    public int currentMenuIndex;
    public int currentHorizontal;
    public int menusIndex;

    private bool canMove;
    private bool canMoveHorizontally;
    
    private InputManager input;
    private LanguageManager languageManager;
    private DisplaySettings displaySettings;
    private UiManagerPlayer uiManager;
    private MainManager gm;
    private float goUp;
    private float goDown;
    private float moveInput;
    
    private void Start()
    {
        input = InputManager.GetInstance();
        languageManager = LanguageManager.GetInstance();
        displaySettings = DisplaySettings.GetInstance();
        uiManager = UiManagerPlayer.GetInstance();
        gm = MainManager.GetInstance();
    }

    private void OnEnable()
    {
        InitState();
        SelectCurrentOption();
    }

    private void InitState()
    {
        canMoveHorizontally = true;
        currentMenu = pauseMenu;
        menusIndex = 0;
        currentMenuIndex = 0;
        canMove = true;
    }

    private bool flag;
    
    private void Update()
    {
        if (input == null || currentMenu == null || currentMenu.Length == 0 || gm.countDownActive) return;
        if (gm.gameState == GameState.Play)
        {
            flag = true;
            return;
        }

        if (flag)
        {
            InitState();
            flag = false;
        }
        
        HandleVerticalNavigation();
        HandleHorizontalNavigation();
        HandleSubmit();
    }
    
    private void HandleVerticalNavigation()
    {
        float verticalStick = input.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        float verticalDpad = input.GetAXis(AXIS.VERTICAL_DPAD);

        float vertical = Mathf.Abs(verticalDpad) > Mathf.Abs(verticalStick)
            ? verticalDpad
            : verticalStick;

        if (vertical > 0 && canMove)
        {
            canMove = false;
            currentMenuIndex--;

            if (currentMenuIndex < 0)
                currentMenuIndex = currentMenu.Length - 1;

            SelectCurrentOption();
        }
        else if (vertical < 0 && canMove)
        {
            canMove = false;
            currentMenuIndex++;

            if (currentMenuIndex > currentMenu.Length - 1)
                currentMenuIndex = 0;

            SelectCurrentOption();
        }
        else if (vertical == 0)
        {
            canMove = true;
        }
    }
    
    private void HandleSubmit()
    {
        if (!input.IsButtonDown(BUTTONS.B)) return;

        Selectable current = currentMenu[currentMenuIndex];
        
        MenusActions();
    }
    
    private void HandleHorizontalNavigation()
    {
        float horizontalStick = input.GetAXis(AXIS.LEFT_STICK_HORIZONTAL);
        float horizontalDpad = input.GetAXis(AXIS.HORIZONTAL_DPAD);

        float horizontal = Mathf.Abs(horizontalDpad) > Mathf.Abs(horizontalStick)
            ? horizontalDpad
            : horizontalStick;

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
        if (menusIndex != 1) return;
        
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

    private void MenusActions()
    {
        switch (menusIndex)
        {
            case 0:
            {
                PauseMenuActions();
                break;
            }
            case 1:
            {
                SettingsMenuActions();
                break;
            }
        }
        currentMenuIndex = 0;
        SelectCurrentOption();
    }

    private void PauseMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
            {
                gm.ChangeGameState(GameState.Play);
                uiManager.ResumeGame();
                break;
            }
            case 1:
            {
                uiManager.GoToSettings();
                currentMenu = settingsMenu;
                menusIndex = 1;
                break;
            }
            case 2:
            {
                SceneLoader.GetInstance()?.LoadScene(0);
                break;
            }
        }
    }

    private void SettingsMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 4:
            {
                uiManager.PauseGame();
                menusIndex = 0;
                currentMenu = pauseMenu;
                break;
            }
        }
    }
    


    private void SelectCurrentOption()
    {
        if (currentMenu[currentMenuIndex] == null) return;
        currentMenu[currentMenuIndex].Select();
    }
}
