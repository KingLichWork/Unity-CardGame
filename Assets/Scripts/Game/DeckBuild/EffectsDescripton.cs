using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
public struct EffectDescription
{
    public Sprite EffectImage;

    public string NameEng;
    public string NameRu;

    public string DescriptionEng;
    public string DescriptionRu;

    public EffectDescription(string effectImagePath, string nameEng, string nameRu, string descriptionEng, string descriptionRu)
    {
        EffectImage = Resources.Load<SpriteAtlas>("Sprites/Effects/EffectSpiteAtlas").GetSprite(effectImagePath);
        NameEng = nameEng;
        NameRu = nameRu;

        DescriptionEng = descriptionEng;
        DescriptionRu = descriptionRu;
    }
}

public static class CardEffectsDescriptionList
{
    public static List<EffectDescription> effectDescriptionList = new List<EffectDescription>();
}

public class EffectsDescripton : MonoBehaviour
{
    private void Awake()
    {
        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Destroy",
            "Destroy",
            "Уничтожьте",
            "Сhange card points to 0.",
            "Измените очки карты до 0."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Damage",
            "Damage",
            "Урон",
            "Сhange card points to - value.",
            "Измените очки карты на - значение."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Boost",
            "Boost",
            "Усиление",
            "Сhange card points to + value.",
            "Измените очки карты на + значение."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Spawn",
            "Spawn",
            "Создание",
            "Сreate a unit.",
            "Создайте отряд."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Draw",
            "Draw card",
            "Добор карты",
            "Add first card from deck to your hand.",
            "Добавьте первую карту из колоды в вашу руку."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Near",
            "Near",
            "Рядом",
            "Cards to the left and right of the selected one.",
            "Карты слева и справа от выбранной."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Armor",
            "Armor",
            "Броня",
            "Block Damage",
            "Блокирует урон."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Stun",
            "Stun",
            "Оглушение",
            "The target's end of turn abilities are disabled for 1 turn.",
            "Способности цели в конце хода отключены на 1 ход."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Shield",
            "Shield",
            "Щит",
            "Blocks 1 tick of damage.",
            "Блокирует 1 получение урона."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Illusion",
            "Illusion",
            "Иллюзия",
            "Receives 2 times more damage.",
            "Получает в 2 раза больше урона."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Invisibility",
            "Invisibility",
            "Невидимость",
            "The card must be played onto the enemy field.",
            "Карта должна быть сыграна на поле врага."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Invulnerability",
            "Invulnerability",
            "Неуязвимость",
            "Card cannot be targeted.",
            "Карта не может быть выбрана целью."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Bleeding",
            "Bleeding",
            "Кровотечение",
            "At the end of your turn damage card by 1 and change duration -1.",
            "В конце вашего хода наносите карте 1 урон и уменьшите продолжительность на 1."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Endurance",
            "Endurance",
            "Выносливость",
            "At the end of your turn boost card by 1 and change duration -1.",
            "В конце вашего хода увеличьте очки карты на 1 и уменьшите продолжительность на 1."
        ));

        CardEffectsDescriptionList.effectDescriptionList.Add(new EffectDescription(
            "Timer",
            "Timer",
            "Таймер",
            "At the end of your turn change value timer by -1, if it becomes 0, apply the effect.",
            "В конце вашего хода уменьшите значение таймера на 1, если оно станет 0, примените эффект."
        ));
    }
}


