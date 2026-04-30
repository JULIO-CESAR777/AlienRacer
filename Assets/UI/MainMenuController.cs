using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    
    public static MainMenuController instance;
    public static MainMenuController GetInstance() => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    [Header("Camera Controller")]
    [SerializeField] private MenuCameraController cameraController;
    [SerializeField] private MenuAnimatorController playAnims;

    [Header("Sistema de guardado")]
    [SerializeField] private SaveSlotButtonHandler saveSlot;

    private LanguageManager languageManager;
    private DisplaySettings displaySettings;
    
    [Header("Menus")]
    public Selectable[] mainMenu;
    public Selectable[] playMenu;
    public Selectable[] slotsMenu;
    public Selectable[] slotsBorrarMenu;
    public Selectable[] settingsMenu;

    private Selectable[] currentMenu;

    public int currentMenuIndex;
    public int menusIndex;

    private bool canMove;
    private bool canMoveHorizontally;
    public bool isOnAnim;

    private InputManager input;
    
    private void Start()
    {
        displaySettings = DisplaySettings.GetInstance();
        input = InputManager.GetInstance();
        languageManager = LanguageManager.GetInstance();

        canMove = true;
        canMoveHorizontally = true;
        isOnAnim = false;

        ChangeMenu(mainMenu, 0, 0);
    }

    private void Update()
    {
        if (input == null || currentMenu == null || currentMenu.Length == 0) return;

        if (isOnAnim) return;
        
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
            case 1:
                cameraController.GoToMenuFromPlay();
                ChangeMenu(mainMenu, 0, 0);
                return true;

            case 2:
                cameraController.GoToMenuFromSettings();
                ChangeMenu(mainMenu, 0, 0);
                return true;

            case 4:
                playAnims.CerrarMenu();
                ChangeMenu(playMenu, 1, 0);
                return true;

            case 5:
                playAnims.CerrarMenuBorrado();
                ChangeMenu(slotsMenu, 4, 0);
                return true;
        }

        return false;
    }

    private void HandleVerticalNavigation()
    {
        float verticalStick = input.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        float verticalDpad = input.GetAXis(AXIS.VERTICAL_DPAD);

        // Tomamos el input más fuerte (stick o dpad)
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
        float horizontalStick = input.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        float horizontalDpad = input.GetAXis(AXIS.HORIZONTAL_DPAD);

        // Tomamos el input más fuerte (stick o dpad)
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

        MenusActions(menusIndex);
    }

    private void OnHorizontalInput(bool right)
    {
        if (menusIndex != 2) return;

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

    private void MenusActions(int index)
    {
        switch (index)
        {
            case 0:
                MainMenuActions();
                break;

            case 1:
                PlayMenuActions();
                break;

            case 2:
                SettingsMenuActions();
                break;

            case 4:
                PlaySlotsActions();
                break;

            case 5:
                EraseSlotsActions();
                break;
        }

        SelectCurrentOption();
    }

    private void MainMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
                cameraController.GoToPlay();
                ChangeMenu(playMenu, 1, 0);
                break;

            case 1:
                cameraController.GoToSettings();
                ChangeMenu(settingsMenu, 2, 0);
                break;

            case 2:
                Debug.Log("Controles / Quit temporal");
                Application.Quit();
                break;

            case 3:
                Application.Quit();
                break;
        }
    }

    private void PlayMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
                // Tutorial
                break;

            case 1:
                playAnims.AbrirMenu();
                ChangeMenu(slotsMenu, 4, 0);
                break;

            case 2:
                cameraController.GoToMenuFromPlay();
                ChangeMenu(mainMenu, 0, 0);
                break;
        }
    }

    private void PlaySlotsActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
                saveSlot.SelectSlot(0);
                break;

            case 1:
                saveSlot.SelectSlot(1);
                break;

            case 2:
                saveSlot.SelectSlot(2);
                break;

            case 3:
                playAnims.CerrarMenu();
                ChangeMenu(playMenu, 1, 0);
                break;

            case 4:
                playAnims.AbrirMenuBorrado();
                RefreshDeleteSlotsMenu();
                ChangeMenu(slotsBorrarMenu, 5, 0);
                break;
        }
    }

    private void EraseSlotsActions()
    {
        switch (currentMenuIndex)
        {
            case 0:
                saveSlot.DeleteSlot(0);
                RefreshDeleteSlotsMenu();
                FixCurrentIndexAfterRefresh();
                break;

            case 1:
                saveSlot.DeleteSlot(1);
                RefreshDeleteSlotsMenu();
                FixCurrentIndexAfterRefresh();
                break;

            case 2:
                saveSlot.DeleteSlot(2);
                RefreshDeleteSlotsMenu();
                FixCurrentIndexAfterRefresh();
                break;

            case 3:
                playAnims.CerrarMenuBorrado();
                ChangeMenu(slotsMenu, 4, 0);
                break;
        }
    }

    private void SettingsMenuActions()
    {
        switch (currentMenuIndex)
        {
            case 4:
                cameraController.GoToMenuFromSettings();
                ChangeMenu(mainMenu, 0, 0);
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

    private void RefreshDeleteSlotsMenu()
    {
        if (slotsBorrarMenu == null) return;

        for (int i = 0; i < slotsBorrarMenu.Length; i++)
        {
            if (slotsBorrarMenu[i] == null) continue;

            if (i >= 0 && i <= 2)
                slotsBorrarMenu[i].interactable = SlotSaveSystem.SlotHasData(i);
            else
                slotsBorrarMenu[i].interactable = true;
        }
    }

    public void ChangeIsOnAnim()
    {
        isOnAnim = !isOnAnim;
    }
    
}