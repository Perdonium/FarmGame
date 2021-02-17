using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Experimental.Rendering.Universal;
using Prime31.MessageKit;

namespace FarmGame
{
    public class LightManager : MonoBehaviour
    {
        [SerializeField]
        Transform nightLightParent;
        List<Light2D> nightLights;

        [SerializeField]
        Light2D dayLight;

        bool inTransition;
        bool toNight;
        float lightTransitionValue = 0;
        const float transitionSpeed = 1.5f;

        // Start is called before the first frame update
        void Start()
        {
            nightLights = nightLightParent.GetComponentsInChildren<Light2D>().ToList();

            MessageKit<bool>.addObserver(Messages.NightSwitch, (b) => OnNightSwitch(b));
        }

        void OnNightSwitch(bool night){
            inTransition = true;
            toNight = night;
            lightTransitionValue = 0;
            DOTween.To(() => lightTransitionValue, x => lightTransitionValue = x, 1, 2).OnComplete(() => inTransition = false);
            
        }

        // Update is called once per frame
        void Update()
        {
            if(inTransition){
                if(toNight){
                    dayLight.intensity = Mathf.Clamp(1-lightTransitionValue,0.4f,1f);

                    for(int i=0;i<nightLights.Count;i++){
                        nightLights[i].intensity = lightTransitionValue;
                    }
                } else {
                    dayLight.intensity = Mathf.Clamp(lightTransitionValue,0.4f, 1f);

                    for(int i=0;i<nightLights.Count;i++){
                        nightLights[i].intensity = 1-lightTransitionValue;
                    }                 
                }
            }
        }
    }
}
