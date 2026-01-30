using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public class MenuPanel : UIPanel
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _exitButton;

    private SettingsPanel _settingsPanel;

    [Inject]
    private void Construct(SettingsPanel settingsPanel)
    {
        _settingsPanel = settingsPanel;
    }

    private void OnEnable()
    {
        _startGameButton.onClick.AddListener(StartGame);
        _settingsButton.onClick.AddListener(Settings);
        _exitButton.onClick.AddListener(Exit);
    }

    private void OnDisable()
    {
        _startGameButton.onClick.RemoveListener(StartGame);
        _settingsButton.onClick.RemoveListener(Settings);
        _exitButton.onClick.RemoveListener(Exit);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("DeckBuild");
    }

    private void Settings()
    {
        _settingsPanel.Show();
    }

    private void Exit()
    {
        Application.Quit();
    }
}
