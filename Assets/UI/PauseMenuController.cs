using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menus")]
    public Selectable[] pauseMenu;
    public Selectable[] settingsMenu;

    private Selectable[] currentMenu;

    public int currentMenuIndex;
    public int menusIndex;

    private bool canMove;
    private bool canMoveHorizontally;

    private InputManager input;
    private LanguageManager languageManager;
    private DisplaySettings displaySettings;
    private UiManagerPlayer uiManager;
    private MainManager gm;

    private bool flag;

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
        ChangeMenu(pauseMenu, 0, 0);
    }

    private void Update()
    {
        if (input == null || currentMenu == null || currentMenu.Length == 0) return;
        if (gm == null || gm.countDownActive) return;

        if (gm.gameState == GameState.Play)
        {
            flag = true;
            return;
        }

        if (flag)
        {
            ChangeMenu(pauseMenu, 0, 0);
            flag = false;
        }

        if (HandleBackSubmit()) return;

        HandleVerticalNavigation();
        HandleHorizontalNavigation();
        HandleSubmit();
    }

    private bool HandleBackSubmit()
    {
        if (!input.IsButtonDown(BUTTONS.X)) return false;

        switch (menusIndex)
        {
            case 0:
                gm.ChangeGameState(GameState.Play);
                uiManager.ResumeGame();
                return true;
            case 1: // Settings -> Pause menu
                uiManager.PauseGame();
                ChangeMenu(pauseMenu, 0, 0);
                return true;
        }

        return false;
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
            MoveSelection(-1);
            SelectCurrentOption();
        }
        else if (vertical < 0 && canMove)
        {
            canMove = false;
            MoveSelection(1);
            SelectCurrentOption();
        }
        else if (vertical == 0)
        {
            canMove = true;
        }
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

    private void HandleSubmit()
    {
        if (!input.IsButtonDown(BUTTONS.B)) return;
        if (!IsValidSelectableIndex(currentMenuIndex)) return;

        MenusActions();
    }

    private void OnHorizontalInput(bool right)
    {
        if (menusIndex != 1) return;

        switch (currentMenuIndex)
        {
            case 0:
                languageManager.ChangeLanguage(right);
                break;

            case 1:
                displaySettings.ChangeMode(right);
                break;

            case 2:
            case 3:
                Slider slider = currentMenu[currentMenuIndex].GetComponent<Slider>();
                if (slider != null)
                {
                    slider.value += right ? 0.1f : -0.1f;
                }
                break;
        }
    }

    private void MenusActions()
    {
        switch (menusIndex)
        {
            case 0:
                PauseMenuActions();
                break;

            case 1:
                SettingsMenuActions();
                break;
        }

        SelectCurrentOption();
    }

    private void PauseMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
                gm.ChangeGameState(GameState.Play);
                uiManager.ResumeGame();
                break;

            case 1:
                uiManager.GoToSettings();
                ChangeMenu(settingsMenu, 1, 0);
                break;

            case 2:
                SceneLoader.GetInstance()?.LoadScene(0);
                break;
        }
    }

    private void SettingsMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 4:
                uiManager.PauseGame();
                ChangeMenu(pauseMenu, 0, 0);
                break;
        }
    }

    private void ChangeMenu(Selectable[] newMenu, int newMenuIndex, int startIndex)
    {
        currentMenu = newMenu;
        menusIndex = newMenuIndex;
        currentMenuIndex = startIndex;

        canMove = false;
        canMoveHorizontally = false;

        FixCurrentIndexAfterRefresh();
        SelectCurrentOption();
    }

    private void MoveSelection(int direction)
    {
        if (currentMenu == null || currentMenu.Length == 0) return;

        int attempts = 0;

        do
        {
            currentMenuIndex += direction;

            if (currentMenuIndex < 0)
                currentMenuIndex = currentMenu.Length - 1;
            else if (currentMenuIndex >= currentMenu.Length)
                currentMenuIndex = 0;

            attempts++;

            if (IsValidSelectableIndex(currentMenuIndex))
                return;

        } while (attempts < currentMenu.Length);
    }

    private void SelectCurrentOption()
    {
        if (!IsValidSelectableIndex(currentMenuIndex))
        {
            currentMenuIndex = GetFirstValidSelectableIndex();
        }

        if (currentMenuIndex == -1) return;

        currentMenu[currentMenuIndex].Select();
    }

    private bool IsValidSelectableIndex(int index)
    {
        if (currentMenu == null) return false;
        if (index < 0 || index >= currentMenu.Length) return false;
        if (currentMenu[index] == null) return false;

        return currentMenu[index].interactable;
    }

    private int GetFirstValidSelectableIndex()
    {
        if (currentMenu == null) return -1;

        for (int i = 0; i < currentMenu.Length; i++)
        {
            if (IsValidSelectableIndex(i))
                return i;
        }

        return -1;
    }

    private void FixCurrentIndexAfterRefresh()
    {
        if (!IsValidSelectableIndex(currentMenuIndex))
            currentMenuIndex = GetFirstValidSelectableIndex();
    }
}