using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInDeck : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _points;
    [SerializeField] private TextMeshProUGUI _name;

    [SerializeField] private Image _image;

    public void SetInfo(string name, string points, Sprite sprite)
    {
        _name.text = name;
        _points.text = points;
        _image.sprite = sprite;
    }
}
