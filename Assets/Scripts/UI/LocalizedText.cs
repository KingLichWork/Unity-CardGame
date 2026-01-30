using TMPro;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private bool _isIdentically;

    [SerializeField] private string _en;
    [SerializeField] private string _ru;

    private TextMeshProUGUI _text;

    private LocalizationManager _localizationManager;

    [Inject]
    private void Construct(LocalizationManager localizationManager)
    {
        _localizationManager = localizationManager;
    }

    //private void OnEnable()
    //{
    //    SettingsPanel.ChangeLanguage += UpdateText;
    //}

    //private void OnDisable()
    //{
    //    SettingsPanel.ChangeLanguage -= UpdateText;
    //}

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    //private void Start()
    //{
    //    if (_localizationManager.Language == Languages.En || _isIdentically)
    //        _text.text = _en;
    //    else if (_localizationManager.Language == Languages.Ru)
    //        _text.text = _ru;
    //}

    //public void UpdateText()
    //{
    //    if (_localizationManager.Language == Languages.En || _isIdentically)
    //        _text.text = _en;
    //    else if (_localizationManager.Language == Languages.Ru)
    //        _text.text = _ru;
    //}
}

