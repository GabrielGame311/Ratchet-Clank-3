using UnityEngine;

public class MenuMouseLocked : MonoBehaviour
{

    public static MenuMouseLocked instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Locked()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }
    public void Unlocked()
    {
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

}
