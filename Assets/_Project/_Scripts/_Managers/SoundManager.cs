using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31.MessageKit;
using DG.Tweening;

namespace FarmGame
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField]
        AudioSource musicSource;

        [SerializeField]
        AudioSource soundSource;

        [SerializeField]
        AudioClip positiveEvent;
        private void Start()
        {
            MessageKit<bool>.addObserver(Messages.NightSwitch,(b) => OnNightSwitch(b));

            MessageKit.addObserver(Messages.PositiveEvent, () => {soundSource.clip = positiveEvent; soundSource.Play();});
        }

        void OnNightSwitch(bool night){
            if(night){
                musicSource.DOFade(0.2f,0.5f);
            } else {
                musicSource.DOFade(1f,0.5f);
            }
        }
    }
}