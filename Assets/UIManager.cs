using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Prime31.MessageKit;


namespace FarmGame
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        GameObject topViewCanvas;

        // Start is called before the first frame update
        void Start()
        {

            MessageKit.addObserver(Messages.SwitchToTopView, () => topViewCanvas.SetActive(true));
            MessageKit.addObserver(Messages.SwitchToField, () => topViewCanvas.SetActive(false));

        }
    }

}