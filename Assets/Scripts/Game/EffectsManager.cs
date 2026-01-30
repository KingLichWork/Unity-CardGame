using DG.Tweening;
using System.Collections;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public Material DestroyMaterial;
    public Material ShieldMaterial;
    public Material IllusionMaterial;
    public Material InvisibilityMaterial;
    public Material InvulnerabilityMaterial;

    public GameObject CardBackPlayer;
    public GameObject CardBackEnemy;
    public Transform PlayerDeck;
    public Transform EnemyDeck;

    [HideInInspector] public int ParticleZCoordinate = 50;
    [HideInInspector] public float ParticleTimeToMove = 0.4f;
    [HideInInspector] public float ShaderChangePointsTime = 1f;

    [SerializeField] private ParticleSystem[] _damageParticle;
    [SerializeField] private ParticleSystem[] _damageBurstParticle;

    [SerializeField] private ParticleSystem[] _boostParticle;
    [SerializeField] private ParticleSystem[] _boostBurstParticle;

    public void DrawCardEffect(float time, Transform Hand, bool isPlayer)
    {
        if (isPlayer)
        {
            CardBackPlayer.SetActive(true);
            CardBackPlayer.transform.position = PlayerDeck.transform.position;
            CardBackPlayer.transform.DOMove(Hand.position, time);
        }
        else
        {
            CardBackEnemy.SetActive(true);
            CardBackEnemy.transform.position = EnemyDeck.transform.position;
            CardBackEnemy.transform.DOMove(Hand.position, time);
        }
    }

    public void HideDrawCardEffect()
    {
        CardBackPlayer.SetActive(false);
        CardBackEnemy.SetActive(false);
    }

    public void StartParticleEffects(Transform start, Transform end, int value)
    {
        if (value > 0)
        {
            if (start == end)
                ParticleEffects(start, end, true, true);
            else
                ParticleEffects(start, end, true, false, true);
        }
        else
        {
            if (start == end)
                ParticleEffects(start, end, false, true);
            else
                ParticleEffects(start, end, false, false, true);
        }
    }

    private void ParticleEffects(Transform start, Transform end, bool isBoost, bool isSelf, bool isStartDelay = false)
    {
        if (isBoost)
        {
            for (int i = 0; i < 9; i++)
            {
                if (!_boostParticle[i].isPlaying)
                {
                    if (!isSelf)
                    {

                        _boostParticle[i].transform.position = new Vector3(start.position.x, start.position.y, ParticleZCoordinate);
                        _boostParticle[i].Play();
                        _boostParticle[i].transform.DOMove(new Vector3(end.position.x, end.position.y, ParticleZCoordinate), ParticleTimeToMove);

                        if (isStartDelay)
                            _boostBurstParticle[i].startDelay = ParticleTimeToMove;
                        _boostBurstParticle[i].transform.position = new Vector3(end.position.x, end.position.y, ParticleZCoordinate);
                        _boostBurstParticle[i].Play();
                        break;
                    }
                    else
                    {
                        _boostBurstParticle[i].transform.position = new Vector3(start.position.x, start.position.y, ParticleZCoordinate);
                        _boostBurstParticle[i].Play();
                        break;
                    }
                }
            }
        }

        if (!isBoost)
        {
            for (int i = 0; i < 9; i++)
            {
                if (!_damageParticle[i].isPlaying)
                {
                    if (!isSelf)
                    {
                        _damageParticle[i].transform.position = new Vector3(start.position.x, start.position.y, ParticleZCoordinate);
                        _damageParticle[i].Play();
                        _damageParticle[i].transform.DOMove(new Vector3(end.position.x, end.position.y, ParticleZCoordinate), ParticleTimeToMove);

                        if (isStartDelay)
                            _damageBurstParticle[i].startDelay = ParticleTimeToMove;
                        _damageBurstParticle[i].transform.position = new Vector3(end.position.x, end.position.y, ParticleZCoordinate);
                        _damageBurstParticle[i].Play();
                        break;
                    }
                    else
                    {
                        _damageBurstParticle[i].transform.position = new Vector3(start.position.x, start.position.y, ParticleZCoordinate);
                        _damageBurstParticle[i].Play();
                        break;
                    }
                }
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
        yield return new WaitForSeconds(ParticleTimeToMove);

        float damage = ShaderChangePointsTime;
        card.Image.material.SetFloat("_Damage", damage);
        card.Image.material.SetColor("_Color", color);

        if (math.abs(value) > 12)
            card.Image.material.SetFloat("_Value", 0);
        else
            card.Image.material.SetFloat("_Value", math.abs(value));

        while (damage > 0)
        {
            damage -= 0.05f;
            card.Image.material.SetFloat("_Damage", damage);
            yield return new WaitForSeconds(0.05f);
        }

        card.IsShaderActive = false;

        yield break;
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
        yield return new WaitForSeconds(ParticleTimeToMove);

        float trashold = 0;

        while (trashold <= 1)
        {
            trashold += 0.05f;
            card.DestroyImage.material.SetFloat("_Trashold", trashold);
            yield return new WaitForSeconds(0.05f);
        }

        yield break;
    }
}
