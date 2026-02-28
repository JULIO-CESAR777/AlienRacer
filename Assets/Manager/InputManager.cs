using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
        #region singleton
        public static InputManager inputManager;
    
        public static InputManager GetInstance() => inputManager;
    
        void Awake()
        {
            inputManager = this;
        }
    
        #endregion
    
        // Action para revisar el input type
        INPUT_TYPE currentInputType = INPUT_TYPE.KEYBOARD;
        public Action<INPUT_TYPE> OnChangeInputType;
    
        KeyCode[] KeyboardController =
        {
            KeyCode.K,
            KeyCode.B,
            KeyCode.X,
            KeyCode.Y,
            KeyCode.L,
            KeyCode.R,
            KeyCode.P,
            KeyCode.Tab,
        }; 
    
        KeyCode[] XboxController =
        {
            KeyCode.Joystick1Button0, // A
            KeyCode.Joystick1Button1, // B
            KeyCode.Joystick1Button2, // X
            KeyCode.Joystick1Button3, // Y
            KeyCode.Joystick1Button4, // L1
            KeyCode.Joystick1Button5, // R1
            KeyCode.Joystick1Button6, // Start
            KeyCode.Joystick1Button7, // Select
        };
    
        private KeyCode[] PlaystationController =
        {
            KeyCode.JoystickButton0, // A
            KeyCode.Joystick1Button1, // B,
            KeyCode.Joystick1Button2, // X,
            KeyCode.Joystick1Button3 , // Y
            KeyCode.Joystick1Button4, // L1
            KeyCode.Joystick1Button5, //R1
            KeyCode.Joystick1Button6, //Start
            KeyCode.Joystick1Button7, //Select
        };
        
        KeyCode[][] controllers;
        
        #region Save_Axis
    
        private string[] keyboardAxis =
        {
            "Horizontal", // Left stick horizontal
            "Vertical", // Left stick vertical
            "Mouse X", // Right stick horizontal
            "Mouse Y" // Right stick vertical
        };
        
        private string[] xboxControllerAxis =
        {
            "Horizontal", // Left stick horizontal
            "Vertical", // Left stick vertical
            "Axis5", // Right stick horizontal
            "Axis4", // Right stick vertical
            "Axis9", // LEFT TRIGGER
            "Axis10" // RiGHT TRIGGER
        };
    
        private string[] playstationAxis =
        {
            "Horizontal",
            "Vertical",
            "Axis5",
            "Axis4",
            "Axis9",
            "Axis10"
        };
    
        #endregion
        
    
        private string[][] controllersAxis;
    
        #region  start_And_Updates
        
        void Start()
        {
            controllersAxis = new string[][] {keyboardAxis, xboxControllerAxis, playstationAxis};
            controllers = new KeyCode[][]{KeyboardController, XboxController, PlaystationController};
            CheckAndChangeInputType();
        }
    
        private void Update()
        {
            if (currentInputType == INPUT_TYPE.KEYBOARD)
            {
                if (CheckIfButtonPressOfController())
                {
                    CheckAndChangeInputType();
                }
            }
        }
    
        private int framesToCheckInput = 60;
        
        private void FixedUpdate()
        {
            if (currentInputType != INPUT_TYPE.KEYBOARD)
            {
                framesToCheckInput--;
                if (framesToCheckInput < 0)
                {
                    framesToCheckInput = 60;
                    CheckAndChangeInputType();
                }
            }
        }
        
        private void OnGUI()
        {
            if (currentInputType != INPUT_TYPE.KEYBOARD)
            {
                if (Event.current.isKey)
                {
                    ChangeInputType(INPUT_TYPE.KEYBOARD);
                }
            }
        }
        
        #endregion
    
        // Esta es la funcion que usa la action para poder inscribirse a la action para mandarse a llamar
        void ChangeInputType(INPUT_TYPE _newInputType)
        {
            if (currentInputType == _newInputType) return;
            
            currentInputType = _newInputType;
            print("Current Input Type: " + currentInputType);
            if (OnChangeInputType != null)
            {
                OnChangeInputType(currentInputType);
            }
        }
        
        
        // Cambiar el Input de cualquiera, mas que nada para detectar de primera instancia que tipo de input va a recibir
        void CheckAndChangeInputType()
        {
            string[] controllersConected = Input.GetJoystickNames();
            if((controllersConected == null || controllersConected.Length == 0) ||
            (controllersConected.Length == 1 && (controllersConected[0] == "" || controllersConected[0] == " ")))
            {
                ChangeInputType(INPUT_TYPE.KEYBOARD);
            }else{ 
                currentInputType = INPUT_TYPE.XBOX;
    
                for(int i = 0; i < controllersConected.Length; i++)
                {
                    if(controllersConected[i] != "")
                    {
                        if(controllersConected[i].Contains("Pro") || controllersConected[i].Contains("Core"))
                        {
                            ChangeInputType(INPUT_TYPE.KEYBOARD);
                            return;
                        }
                        else if(controllersConected[i].Contains("Wireless"))
                        {
                            ChangeInputType(INPUT_TYPE.PLAYSTATION);
                            return;
                        }
                        else if(controllersConected[i].Contains("Xbox"))
                        {
                            ChangeInputType(INPUT_TYPE.XBOX);
                            return;
                        }
                    }
                }
            }
        }
    
        public bool IsButtonDown(BUTTONS _button)
        {
            bool pressed = Input.GetKeyDown(controllers[(byte)currentInputType][(byte)_button]); 
                
            if (pressed)
            {
                Debug.Log("Button Pressed: " + _button + " | Device: " + currentInputType);
            }    
            return pressed;
        }
        
    
        private float value;
        private float valueAbs;
        public float GetAXis(AXIS _axis)
        { 
            value = Input.GetAxis(controllersAxis[(byte)currentInputType][(byte)_axis]);
            valueAbs = Mathf.Abs(value);
    
            if (valueAbs >= 0.3f)
            {
                Debug.Log("Axis Used: " + _axis + " | Value: " + value + " | Device: " + currentInputType);
                return value;
            }
    
            return 0f;
            
        }
    
        // Chequea si se presiona algun boton de un control
        private const string joystickButtonName = "joystick 1 button ";
        bool CheckIfButtonPressOfController()
        {
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown(joystickButtonName + i))
                {
                    return true;
                }
            }
            return false;
        }   
}

public enum INPUT_TYPE
{
    KEYBOARD,
    XBOX,
    PLAYSTATION,
    SWITCH
    
    
}

public enum BUTTONS
{
    A,
    B,
    X,
    Y,
    L,
    R,
    START,
    SELECT,
}

public enum AXIS
{
    LEFT_STICK_HORIZONTAL,
    LEFT_STICK_VERTICAL,
    RIGHT_STICK_HORIZONTAL,
    RIGHT_STICK_VERTICAL,
    LEFT_TRIGGER,
    RIGHT_TRIGGER
}