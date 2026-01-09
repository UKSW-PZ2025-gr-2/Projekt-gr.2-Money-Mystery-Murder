using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;


    public void StartGame()
    {

        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

   public void ExitGame()
    {

    Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }


}


