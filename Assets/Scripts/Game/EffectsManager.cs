using DG.Tweening;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public Material DestroyMaterial;
    public Material ShieldMaterial;
    public Material IllusionMaterial;
    public Material InvisibilityMaterial;
    public Material InvulnerabilityMaterial;

    [SerializeField] private GameObject _cardBackPlayer;
    [SerializeField] private GameObject _cardBackEnemy;

    [SerializeField] private Transform _playerHand;
    [SerializeField] private Transform _enemyHand;
    [SerializeField] private Transform _playerDeck;
    [SerializeField] private Transform _enemyDeck;

    [SerializeField] private ParticleSystem[] _damageParticle;
    [SerializeField] private ParticleSystem[] _damageBurstParticle;
    [SerializeField] private ParticleSystem[] _boostParticle;
    [SerializeField] private ParticleSystem[] _boostBurstParticle;

    private float _particleTimeToMove = 0.4f;
    private int _particleZCoordinate = 50;
    private float _shaderChangePointsTime = 1f;
    private float _timeDrawCardStart = 0.15f; 
    private float _timeDrawCard = 0.3f;

    public IEnumerator DrawCardEffect(EffectOwner isPlayer, bool isStartDraw)
    {
        float time = isStartDraw ? _timeDrawCardStart : _timeDrawCard;
        GameObject cardBack = isPlayer == EffectOwner.Player ? _cardBackPlayer : _cardBackEnemy;

        cardBack.SetActive(true);
        cardBack.transform.position = isPlayer == EffectOwner.Player ? _playerDeck.transform.position : _enemyDeck.transform.position;
        cardBack.transform.DOMove(isPlayer == EffectOwner.Player ? _playerHand.position : _enemyHand.position, time);

        yield return new WaitForSeconds(time);
    }

    public void HideDrawCardEffect()
    {
        _cardBackPlayer.SetActive(false);
        _cardBackEnemy.SetActive(false);
    }

    public void StartParticleEffects(Transform start, Transform end, int value)
    {
        ParticleEffects(start, end, value > 0, start == end , value > 0 && start != end);
    }

    private void ParticleEffects(Transform start, Transform end, bool isBoost, bool isSelf, bool isStartDelay = false)
    {
        ParticleSystem[] mainParticles = isBoost ? _boostParticle : _damageParticle;
        ParticleSystem[] burstParticles = isBoost ? _boostBurstParticle : _damageBurstParticle;

        for (int i = 0; i < mainParticles.Length; i++)
        {
            if (!mainParticles[i].isPlaying)
            {
                if (!isSelf)
                {
                    mainParticles[i].transform.position = new Vector3(start.position.x, start.position.y, _particleZCoordinate);
                    mainParticles[i].Play();
                    mainParticles[i].transform.DOMove(new Vector3(end.position.x, end.position.y, _particleZCoordinate), _particleTimeToMove);

                    if (isStartDelay)
                        burstParticles[i].startDelay = _particleTimeToMove;

                    burstParticles[i].transform.position = new Vector3(end.position.x, end.position.y, _particleZCoordinate);
                    burstParticles[i].Play();
                }
                else
                {
                    burstParticles[i].transform.position = new Vector3(start.position.x, start.position.y, _particleZCoordinate);
                    burstParticles[i].Play();
                }

                break;
            }
        }
    }

    public void StartShaderEffect(CardInfoScript card, Color color, int value)
    {
        if (!card.IsShaderActive)
        {
            StartCoroutine(ShaderEffect(card, color, value));
            card.IsShaderActive = true;
        }
    }

    private IEnumerator ShaderEffect(CardInfoScript card, Color color, int value)
    {
        yield return new WaitForSeconds(_particleTimeToMove);

        float damage = _shaderChangePointsTime;

        card.Image.material.SetFloat("_Damage", damage);
        card.Image.material.SetColor("_Color", color);
        card.Image.material.SetFloat("_Value", math.abs(value) > 12 ? 0 : math.abs(value));

        while (damage > 0)
        {
            damage -= 0.05f;
            card.Image.material.SetFloat("_Damage", damage);
            yield return new WaitForSeconds(0.05f);
        }

        card.IsShaderActive = false;
    }

    public void StartDestroyCoroutine(CardInfoScript card)
    {
        card.PointObject.SetActive(false);
        card.CardComponents.SetActive(false);
        card.DestroyGameObject.SetActive(true);

        Material destroyMaterial = new Material(DestroyMaterial);
        card.DestroyImage.material = destroyMaterial;
        destroyMaterial.SetTexture("_Image", card.SelfCard.BaseCard.ImageTexture);
        destroyMaterial.SetFloat("_Trashold", 0);

        StartCoroutine(DestroyEffectsCoroutine(card));
    }

    private IEnumerator DestroyEffectsCoroutine(CardInfoScript card)
    {
        yield return new WaitForSeconds(_particleTimeToMove);

        float trashold = 0;

        while (trashold <= 1)
        {
            trashold += 0.05f;
            card.DestroyImage.material.SetFloat("_Trashold", trashold);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
