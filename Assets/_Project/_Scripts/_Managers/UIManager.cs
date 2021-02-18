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
        TextMeshProUGUI money;

        [SerializeField]
        TextMeshProUGUI action;


        [SerializeField]
        Color nightColor;

        [SerializeField]
        Button prepareButton;

        [SerializeField]
        Button plantButton;

        [SerializeField]
        Button harvestButton;

        [SerializeField]
        Button deleteButton;

        [SerializeField]
        Button topCamButton;
        [SerializeField]
        Button resetButton;
        [SerializeField]
        Button toolboxButton;

        [SerializeField]
        RectTransform toolbox;
        bool toolboxOpened = false;
        const float toolboxSpeed = 0.5f;

        [SerializeField]
        RectTransform cropSelection;
        bool cropSelectionOpened = false;
        void Awake()
        {

            MessageKit.addObserver(Messages.SwitchToTopView, () => OnSwitchToTopView() );
            MessageKit.addObserver(Messages.SwitchToFieldView, () =>  OnSwitchToFieldView());

            MessageKit<double>.addObserver(Messages.GameTick, (d) => SetClock(d));
            MessageKit<int>.addObserver(Messages.MoneyUpdate, (i) => SetMoney(i));
            MessageKit<Action>.addObserver(Messages.SwitchAction, (a) => SetAction(a));

            MessageKit<bool>.addObserver(Messages.NightSwitch, (b) => OnNightSwitch(b));

            prepareButton.onClick.AddListener(() => MessageKit<Action>.post(Messages.SwitchAction, Action.Prepare));
            plantButton.onClick.AddListener(() => OpenCropSelection());
            harvestButton.onClick.AddListener(() => MessageKit<Action>.post(Messages.SwitchAction, Action.Harvest));
            deleteButton.onClick.AddListener(() => MessageKit<Action>.post(Messages.SwitchAction, Action.Delete));

            topCamButton.onClick.AddListener(() => MessageKit.post(Messages.SwitchView));
            resetButton.onClick.AddListener(() => MessageKit.post(Messages.ResetPressed));
        
            toolboxButton.onClick.AddListener(() => SwitchToolbox());

        }

        void OnSwitchToTopView(){
            //topViewCanvas.SetActive(true);
            toolbox.gameObject.SetActive(false);
        }

        void OnSwitchToFieldView(){
            //topViewCanvas.SetActive(false);
            toolbox.gameObject.SetActive(true);
        }
        void OpenCropSelection(){
            cropSelection.gameObject.SetActive(true);
            SwitchToolbox();
            cropSelectionOpened = true;
        }
        
        void OnNightSwitch(bool b){
            if(b){
                clock.DOColor(nightColor, 0.5f);
            } else {
                clock.DOColor(Color.white, 0.5f);
            }
        }

        void SwitchToolbox(){
            if(cropSelectionOpened){
                return;
            }
            if(!toolboxOpened){
                toolbox.DOMoveX(-(toolbox.rect.size.x * toolbox.lossyScale.x), toolboxSpeed).SetRelative();
            } else {
                toolbox.DOMoveX(toolbox.rect.size.x * toolbox.lossyScale.x, toolboxSpeed).SetRelative();
            }

            toolboxOpened = !toolboxOpened;
        }

        void SetAction(Action a){
            if(a == Action.Prepare){
                action.text = "Prepare";
            }
            else if(a == Action.Plant){
                action.text = "Plant";
                cropSelection.gameObject.SetActive(false);
                cropSelectionOpened = false;
                return;
            }
            else if(a == Action.Harvest){
                action.text = "Harvest";
            }
            else if(a == Action.Delete){
                action.text = "Delete";
            }

            SwitchToolbox();
        }
        void SetClock(double time)
        {
            double currentHour = (((time % 24) + 8) % 24);
            int day = ((int)time/24)+1;
            clock.text = "Day "+ day+" - "+currentHour.ToString() + "h";
        }

        void SetMoney(int m)
        {
            money.text = m + "$";
        }
    }

}