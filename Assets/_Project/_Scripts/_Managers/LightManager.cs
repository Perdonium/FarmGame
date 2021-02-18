using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Experimental.Rendering.Universal;
using Prime31.MessageKit;

namespace FarmGame
{
    //Manage the lights when switching between day and night
    public class LightManager : MonoBehaviour
    {

        #region Properties

        [SerializeField]
        Transform nightLightParent; //Rather use the parent and generate a list in Start() than a List<Light2D> in the inspector
        List<Light2D> nightLights; //Initialized in Start()

        [SerializeField]
        Light2D dayLight; //The sun

        bool inTransition;
        bool toNight;

        float lightTransitionValue = 0;
        const float transitionSpeed = 1.5f;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            nightLights = nightLightParent.GetComponentsInChildren<Light2D>().ToList();

            MessageKit<bool>.addObserver(Messages.NightSwitch, (b) => OnNightSwitch(b));
        }

        void OnNightSwitch(bool night)
        {
            inTransition = true;
            toNight = night;
            lightTransitionValue = 0;
            //DOTween doesn't implement Light2D so i do it by hand
            DOTween.To(() => lightTransitionValue, x => lightTransitionValue = x, 1, 2).OnComplete(() => inTransition = false);
            StartCoroutine(LightSwitchCoroutine());
        }

        //Here I use a coroutine as a "temporary Update()" to transition the lights
        IEnumerator LightSwitchCoroutine()
        {
            while (inTransition)
            {
                if (toNight)
                {
                    dayLight.intensity = Mathf.Clamp(1 - lightTransitionValue, 0.4f, 1f);

                    for (int i = 0; i < nightLights.Count; i++)
                    {
                        nightLights[i].intensity = lightTransitionValue;
                    }
                }
                else
                {
                    dayLight.intensity = Mathf.Clamp(lightTransitionValue, 0.4f, 1f);

                    for (int i = 0; i < nightLights.Count; i++)
                    {
                        nightLights[i].intensity = 1 - lightTransitionValue;
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }

}
