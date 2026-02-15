using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button settingButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    private void OnEnable()
    {
        playButton.onClick.AddListener(OnPlayButton);
        exitButton.onClick.AddListener(OnExitButton);
        settingButton.onClick.AddListener(OnSettingsButton);
    }
    private void OnDisable()
    {
        playButton.onClick.RemoveListener(OnPlayButton);
        exitButton.onClick.RemoveListener(OnExitButton);
        settingButton.onClick.RemoveListener(OnSettingsButton);
    }
    public void OnPlayButton()
    {

        SceneManager.LoadScene("Game");
    }


    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnSettingsButton()
    {
        Debug.Log("Settings button pressed");
    }
}
