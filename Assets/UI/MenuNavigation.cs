using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    public Button[] MainButtons;
    public Button[] SettingsButtons;
    public Button[] PlayButtons;
    public Button[] SlotsButtons;

    private Button[] currentButtons;

    private int currentIndex = 0;
    private int currentIndexDisplay = 0;
    private float inputDelay = 0.2f;
    private float timer;

    public MenuState currentMenu;

    void Start()
    {
        ChangeToMainMenu();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        float vertical = InputManager.instance.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        float horizontal = InputManager.instance.GetAXis(AXIS.LEFT_STICK_HORIZONTAL);

        if (timer <= 0)
        {
            if (vertical > 0.1f)
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex = currentButtons.Length - 1;

                SelectButton(currentIndex);
                timer = inputDelay;
            }
            else if (vertical < -0.1f)
            {
                currentIndex++;
                if (currentIndex >= currentButtons.Length) currentIndex = 0;

                SelectButton(currentIndex);
                timer = inputDelay;
            }

        }

        // BOTÓN A
        if (InputManager.instance.IsButtonDown(BUTTONS.A))
        {
            currentButtons[currentIndex].onClick.Invoke();
        }
        
    }

    void SelectButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(currentButtons[index].gameObject);
    }

    public void ChangeToSettings()
    {
        currentMenu = MenuState.SETTINGS;
        currentIndex = 0;
        currentButtons = SettingsButtons;
        
        SelectButton(currentIndex);
    }

    public void ChangeToMainMenu()
    {
        currentMenu = MenuState.MAIN;
        currentIndex = 0;
        currentButtons = MainButtons;
        SelectButton(currentIndex);
    }

    public void ChangeToPlay()
    {
        currentMenu = MenuState.PLAY;
        currentIndex = 0;
        currentButtons = PlayButtons;
        SelectButton(currentIndex);
    }

    public void ChangeToSlots()
    {
        currentMenu = MenuState.SLOTS;
        currentIndex = 0;
        currentButtons = SlotsButtons;
        SelectButton(currentIndex);
    }
    
}

public enum MenuState
{
    MAIN,
    SETTINGS,
    PLAY,
    SLOTS
}
