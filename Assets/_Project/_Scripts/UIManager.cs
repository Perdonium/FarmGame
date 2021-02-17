using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Prime31.MessageKit;
using TMPro;
using DG.Tweening;

namespace FarmGame
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        GameObject topViewCanvas;

        [SerializeField]
        TextMeshProUGUI clock;

        [SerializeField]
        Color nightColor;

        // Start is called before the first frame update
        void Start()
        {

            MessageKit.addObserver(Messages.SwitchToTopView, () => topViewCanvas.SetActive(true));
            MessageKit.addObserver(Messages.SwitchToField, () => topViewCanvas.SetActive(false));

            MessageKit<double>.addObserver(Messages.GameTick, (d) => SetClock(d));
        }

        void SetClock(double time){
            double currentHour = (((time%24)+8)%24);

            if(currentHour==GlobalVariables.nightStart){
                clock.DOColor(nightColor,0.5f);
            } else if(currentHour==GlobalVariables.nightEnd){
                clock.DOColor(Color.white,0.5f);
            }
            clock.text = currentHour.ToString()+"h";
        }
    }

}