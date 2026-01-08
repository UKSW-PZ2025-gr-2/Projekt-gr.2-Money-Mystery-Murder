using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public Slider volumeSlider;

    //bool paused = true;

    //void Start()
    //{
    //    Time.timeScale = 0f;
    //    menuPanel.SetActive(true);
    //    volumeSlider.value = AudioListener.volume;
    //}

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //        TogglePause();
    //}

    public void StartGame()
    {
        //paused = false;
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    //public void TogglePause()
    //{
    //    paused = !paused;
    //    menuPanel.SetActive(paused);
    //    Time.timeScale = paused ? 0f : 1f;
    //}

    public void ChangeVolume(float v)
    {
        AudioListener.volume = v;
    }

   public void ExitGame()
{
    // Działa w buildzie
    Application.Quit();

    // Działa w Unity Editor
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#endif
}

//    public void Resume()
//    {
//        paused = false;
//        if(menuPanel != null)
//
//            menuPanel.SetActive(false);
//        Time.timeScale = 1f;
//    }
}


