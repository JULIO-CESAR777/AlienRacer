using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    public static MenuNavigation instance;
    public static MenuNavigation GetInstance() => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    [Header("Menus")]
    public Selectable[] MainItems;
    public Selectable[] SettingsItems;
    public Selectable[] PlayItems;
    public Selectable[] SlotsItems;

    private Selectable[] currentItems;

    [Header("Slider Settings")]
    [SerializeField] private float sliderStep = 0.1f;

    [Header("Input Thresholds")]
    [SerializeField] private float pressThreshold = 0.5f;
    [SerializeField] private float releaseThreshold = 0.0f;
    
    public int currentIndex = 0;

    private bool verticalHeld = false;
    private bool horizontalHeld = false;
    public bool isDoingAnAnimation = false;
    

    public MenuState currentMenu;

    private void Start()
    {
        ChangeToMainMenu();
    }

    private void Update()
    {

        if (isDoingAnAnimation) return;
        
        
        SyncIndexWithSelected();
        
        float vertical = InputManager.instance.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        float horizontal = InputManager.instance.GetAXis(AXIS.LEFT_STICK_HORIZONTAL);

        HandleVertical(vertical);
        HandleHorizontal(horizontal);
        HandleSubmit();
        HandleBack();
    }
    
    private void SyncIndexWithSelected()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || currentItems == null) return;

        for (int i = 0; i < currentItems.Length; i++)
        {
            if (currentItems[i] != null && currentItems[i].gameObject == selected)
            {
                currentIndex = i;
                return;
            }
        }
    }

    private void HandleVertical(float vertical)
    {
        if (vertical == 0f)
        {
            verticalHeld = false;
            return;
        }

        if (verticalHeld) return;

        if (vertical > pressThreshold)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = currentItems.Length - 1;
            SelectCurrent();
            verticalHeld = true;
        }
        else if (vertical < -pressThreshold)
        {
            currentIndex++;
            if (currentIndex >= currentItems.Length) currentIndex = 0;
            SelectCurrent();
            verticalHeld = true;
        }
    }

    private void HandleHorizontal(float horizontal)
    {
        if (horizontal == 0f)
        {
            horizontalHeld = false;
            return;
        }

        if (horizontalHeld) return;

        if (currentMenu != MenuState.SETTINGS) return;

        Selectable current = currentItems[currentIndex];

        if (currentIndex == 0)
        {
            LanguageManager.instance.ChangeLanguage(horizontal > 0);
            horizontalHeld = true;
            return;
        }

        if (currentIndex == 1)
        {
            DisplaySettings.instance.ChangeMode(horizontal > 0);
            horizontalHeld = true;
            return;
        }

        Slider slider = current.GetComponent<Slider>();
        if (slider != null)
        {
            float direction = horizontal > 0 ? 1f : -1f;
            slider.value += direction * sliderStep;
            horizontalHeld = true;
        }
    }

    private void HandleSubmit()
    {
        if(isDoingAnAnimation) return;
        if (!InputManager.instance.IsButtonDown(BUTTONS.A)) return;

        Selectable current = currentItems[currentIndex];

        Button button = current.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.Invoke();
            return;
        }
    }

    private void HandleBack()
    {
        if(isDoingAnAnimation) return;
        if (!InputManager.instance.IsButtonDown(BUTTONS.B)) return;

        switch (currentMenu)
        {
            case MenuState.SETTINGS:
            case MenuState.PLAY:
            case MenuState.SLOTS:
                ChangeToMainMenu();
                break;
        }
    }
    
    private void SetCurrentMenuInteractable(bool value)
    {
        if (currentItems == null) return;

        for (int i = 0; i < currentItems.Length; i++)
        {
            if (currentItems[i] != null)
            {
                currentItems[i].interactable = value;
            }
        }
    }

    public void DoingAnAnimation()
    {
        isDoingAnAnimation = true;
        SetCurrentMenuInteractable(false);
    }


    public void NotDoingAnAnimation()
    {
        print("Animacion terminada");
        isDoingAnAnimation = false;
        SetCurrentMenuInteractable(true);
        SelectCurrent();
    }
    


    private void SelectCurrent()
    {
        if (currentItems == null || currentItems.Length == 0) return;
        if (currentIndex < 0 || currentIndex >= currentItems.Length) return;
        
        EventSystem.current.SetSelectedGameObject(currentItems[currentIndex].gameObject);
    }

    public void ChangeToMainMenu()
    {
        currentMenu = MenuState.MAIN;
        currentItems = MainItems;
        currentIndex = 0;
        SelectCurrent();
        ResetAxisLocks();
    }

    public void ChangeToSettings()
    {
        currentMenu = MenuState.SETTINGS;
        currentItems = SettingsItems;
        currentIndex = 0;
        SelectCurrent();
        ResetAxisLocks();
    }

    public void ChangeToPlay()
    {
        currentMenu = MenuState.PLAY;
        currentItems = PlayItems;
        currentIndex = 0;
        SelectCurrent();
        ResetAxisLocks();
    }

    public void ChangeToSlots()
    {
        currentMenu = MenuState.SLOTS;
        currentItems = SlotsItems;
        currentIndex = 0;
        SelectCurrent();
        ResetAxisLocks();
    }

    private void ResetAxisLocks()
    {
        verticalHeld = false;
        horizontalHeld = false;
    }
}

public enum MenuState
{
    MAIN,
    SETTINGS,
    PLAY,
    SLOTS
}