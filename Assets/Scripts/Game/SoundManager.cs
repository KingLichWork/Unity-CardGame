using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _deploymentPlayerAudioSource;
    [SerializeField] private AudioSource _deploymentEnemyAudioSource;
    [SerializeField] private AudioSource _startPlayerTargetAudioSource;
    [SerializeField] private AudioSource _startEnemyTargetAudioSource;
    [SerializeField] private AudioSource _endTurnAudioSource;
    [SerializeField] private AudioSource _timerAudioSource;

    public void DeploymentSound(CardInfoScript card)
    {
        _deploymentPlayerAudioSource.clip = Resources.Load<AudioClip>("Sounds/Cards/Deployment/" + card.SelfCard.BaseCard.Name + Random.Range(0, 6));
        _deploymentPlayerAudioSource.Play();
    }

    public void StartEffectSound(CardInfoScript card)
    {
        _startPlayerTargetAudioSource.clip = card.SelfCard.BaseCard.CardPlaySound;
        _startPlayerTargetAudioSource.Play();
    }

    public void EndTurnSound(CardInfoScript card)
    {
        _endTurnAudioSource.clip = card.SelfCard.BaseCard.CardPlaySound;
        _endTurnAudioSource.Play();
    }

    public void TimerSound(CardInfoScript card)
    {
        _timerAudioSource.clip = card.SelfCard.BaseCard.CardTimerSound;
        _timerAudioSource.Play();
    }
}
