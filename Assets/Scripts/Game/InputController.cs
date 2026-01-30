using UnityEngine;
using VContainer;

public class InputController : MonoBehaviour
{
    private GameManager _gameManager;
    private UIManager _uiManager;

    [Inject]
    private void Construct(GameManager gameManager, UIManager uiManager)
    {
        _gameManager = gameManager;
        _uiManager = uiManager;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_uiManager.IsPause)
                _uiManager.Pause();
            else
                _uiManager.UnPause();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_uiManager.ReturnEndTurnButtonInteractable())
                _gameManager.StartChangeTurn();
        }
    }
}
