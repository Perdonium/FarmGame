using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31.MessageKit;
using DG.Tweening;

namespace FarmGame
{
    //Basic sound manager
    public class SoundManager : MonoBehaviour
    {
        #region Properties

        [SerializeField]
        AudioSource musicSource;

        [SerializeField]
        AudioSource soundSource;

        [SerializeField]
        AudioClip positiveEvent;

        #endregion
        
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