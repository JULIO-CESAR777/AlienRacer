using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
        #region singleton
        public static InputManager instance;
    
        public static InputManager GetInstance() => instance;
    
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    
        #endregion
    
        // Action para revisar el input type
        public INPUT_TYPE currentInputType = INPUT_TYPE.KEYBOARD;
        public Action<INPUT_TYPE> OnChangeInputType;
    
        KeyCode[] KeyboardController =
        {
            KeyCode.LeftShift, // Drift
            KeyCode.Space, // Salto
            KeyCode.X, // Compra de items
            KeyCode.R, // Sinergia
            KeyCode.Q, // Item 1
            KeyCode.E, // Item 2
            KeyCode.S, // L2
            KeyCode.W, // R2
            KeyCode.P, // Start
            KeyCode.Tab, // Select
        }; 
    
        KeyCode[] XboxController =
        {
            KeyCode.Joystick1Button2, // A
            KeyCode.Joystick1Button0, // B
            KeyCode.Joystick1Button1, // X
            KeyCode.Joystick1Button3, // Y
            KeyCode.Joystick1Button4, // L1
            KeyCode.Joystick1Button5, // R1
            KeyCode.Joystick1Button6, // L2 ---- Falta que funcionen estos
            KeyCode.Joystick1Button7, // R2 ---- Falta que funcionen estos
            KeyCode.Joystick1Button7, // Start
            KeyCode.Joystick1Button6, // Select
        };
    
        private KeyCode[] PlaystationController =
        {
            KeyCode.JoystickButton0, // A
            KeyCode.Joystick1Button1, // B,
            KeyCode.Joystick1Button2, // X,
            KeyCode.Joystick1Button3 , // Y
            KeyCode.Joystick1Button4, // L1
            KeyCode.Joystick1Button5, //R1
            KeyCode.Joystick1Button6, // L2
            KeyCode.Joystick1Button7, // R2
            KeyCode.Joystick1Button9, //Start ---- El joystick6 es el L2
            KeyCode.Joystick1Button10, //Select ---- El joystick7 es el R2
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
            "Axis9", // Left trigger
            "Axis10", // Right trigger
        };
    
        private string[] playstationAxis =
        {
            "Horizontal",
            "Vertical",
            "Axis5",
            "Axis4",
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
    
        public bool IsButton(BUTTONS _button)
        {
            return Input.GetKey(controllers[(byte)currentInputType][(byte)_button]);
        }
        
        public bool IsButtonDown(BUTTONS _button)
        {
            bool pressed = Input.GetKeyDown(controllers[(byte)currentInputType][(byte)_button]); 
                
            if (pressed)
            {
                //Debug.Log("Button Pressed: " + _button + " | Device: " + currentInputType);
            }    
            return pressed;
        }

        public bool IsButtonUp(BUTTONS _button)
        {
            bool pressed = Input.GetKeyUp(controllers[(byte)currentInputType][(byte)_button]); 
                
            if (pressed)
            {
                //Debug.Log("Button Pressed: " + _button + " | Device: " + currentInputType);
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
                //Debug.Log("Axis Used: " + _axis + " | Value: " + value + " | Device: " + currentInputType);
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
    L1,
    R1,
    L2,
    R2,
    START,
    SELECT,
}

public enum AXIS
{
    LEFT_STICK_HORIZONTAL,
    LEFT_STICK_VERTICAL,
    LEFT_TRIGGER,
    RIGHT_TRIGGER
}