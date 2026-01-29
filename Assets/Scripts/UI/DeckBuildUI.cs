using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckBuildUI : UIPanel
{
    [SerializeField] private Button _randomDeckButton;
    [SerializeField] private Button _clearDeckButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _toMenuButton;

    public static event Action RandomDeckAction;
    public static event Action ClearDeckAction;
    public static event Action PlayAction;

    private void OnEnable()
    {
        _randomDeckButton.onClick.AddListener(RandomDeck);
        _clearDeckButton.onClick.AddListener(ClearDeck);
        _playButton.onClick.AddListener(Play);
        _toMenuButton.onClick.AddListener(ToMenu);
    }

    private void OnDisable()
    {
        _randomDeckButton.onClick.RemoveListener(RandomDeck);
        _clearDeckButton.onClick.RemoveListener(ClearDeck);
        _playButton.onClick.RemoveListener(Play);
        _toMenuButton.onClick.RemoveListener(ToMenu);
    }

    private void RandomDeck()
    {
        RandomDeckAction.Invoke();
    }

    private void ClearDeck()
    {
        ClearDeckAction.Invoke();
    }

    private void Play()
    {
        PlayAction.Invoke();
    }

    private void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
