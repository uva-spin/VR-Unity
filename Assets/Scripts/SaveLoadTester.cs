using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class SaveLoadTester : MonoBehaviour
{
    public SaveSystem saveSystem;

    private void Update()
    {
        // Save the game when "S" is pressed
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            saveSystem.Save();
        }

        // Load the game when "L" is pressed
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            saveSystem.Load();
        }
    }
}
