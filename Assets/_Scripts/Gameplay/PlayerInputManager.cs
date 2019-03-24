using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    
    public PlayerController playerController;

    // Determines wheter Input.GetAxis or Input.GetAxisRaw is used
    public bool useRawAxisInput;


    private delegate void AxisInputFunction(float axis);
    private delegate void KeyInputFunction();

    Dictionary<string, KeyInputFunction> keyUpMappings;
    Dictionary<string, KeyInputFunction> keyHeldMappings;
    Dictionary<string, KeyInputFunction> keyDownMappings;
    Dictionary<string, AxisInputFunction> axisMappings;

    // Put default mappings in here, must be called in start
    private void InitialiseMappings()
    {
        if (playerController != null)
        {
            keyUpMappings = new Dictionary<string, KeyInputFunction>()
            {
                { "left shift", playerController.MakePlayerUnbouncy },
                { "left ctrl", playerController.MakePlayerUnsticky }
            };
            keyHeldMappings = new Dictionary<string, KeyInputFunction>()
            {
                { "left shift", playerController.MakePlayerBouncy },
                { "left ctrl", playerController.MakePlayerSticky }
            };
            keyDownMappings = new Dictionary<string, KeyInputFunction>()
            {
                { "space", playerController.Jump },
                //{ "joystick button 0", playerController.Jump }, // A
                { "left shift", playerController.MakePlayerBouncy },
                { "left ctrl", playerController.MakePlayerSticky }
            };
            axisMappings = new Dictionary<string, AxisInputFunction>()
            {
                { "Horizontal", playerController.AccelerateOnInput }
                //{ "XboxRightTrigger", moveController.MakePlayerBouncyTrigger },
                //{ "XboxLeftTrigger", moveController.MakePlayerStickyTrigger }
            };
        }
        else
        {
            keyUpMappings = new Dictionary<string, KeyInputFunction>();
            keyHeldMappings = new Dictionary<string, KeyInputFunction>();
            keyDownMappings = new Dictionary<string, KeyInputFunction>();
            axisMappings = new Dictionary<string, AxisInputFunction>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();        InitialiseMappings();
    }

    // Update is called once per frame
    void Update()
    {
        // Call Axis functions with appropriate input
        if (axisMappings != null) CallAxisFunctions();

        // Call Button Up methods for keys pressed
        if (keyUpMappings != null) CallKeyUpFunctions();

        // Call GetKey methods for keys pressed
        if (keyHeldMappings != null) CallKeyHeldFunctions();

        // Call Key Downmethods for keys pressed
        if (keyDownMappings != null) CallKeyDownFunctions();

    }

    void CallAxisFunctions()
    {
        foreach (string axis in axisMappings.Keys)
        {
            float axisValue = 0;
            if (useRawAxisInput)
            {
                axisValue = Input.GetAxisRaw(axis);
            }
            else
            {
                axisValue = Input.GetAxis(axis);
            }
            axisMappings[axis](axisValue);
        }
    }

    void CallKeyUpFunctions()
    {
        foreach (string key in keyUpMappings.Keys)
        {
            if (Input.GetKeyUp(key)) keyUpMappings[key]();

        }
    }

    void CallKeyHeldFunctions()
    {
        foreach (string key in keyHeldMappings.Keys)
        {
            if (Input.GetKey(key)) keyHeldMappings[key]();
        }
    }

    void CallKeyDownFunctions()
    {
        foreach (string key in keyDownMappings.Keys)
        {
            if (Input.GetKeyDown(key)) keyDownMappings[key]();
        }
    }
}
