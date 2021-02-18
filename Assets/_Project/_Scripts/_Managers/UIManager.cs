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
        
        #region Properties

        [SerializeField]
        GameObject worldSpaceCanvas;

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
        const float toolboxSpeed = 0.5f; //The toolbox expand/collapse speed

        [SerializeField]
        RectTransform cropSelection;
        bool cropSelectionOpened = false;

        #endregion

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
            toolbox.gameObject.SetActive(false);
        }

        void OnSwitchToFieldView(){
            toolbox.gameObject.SetActive(true);
        }
        void OpenCropSelection(){
            Vector3 cropSelectionScale = cropSelection.localScale;
            cropSelection.localScale = Vector3.zero;
            cropSelection.gameObject.SetActive(true);
            cropSelection.DOScale(cropSelectionScale, 0.5f);

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
            //Move the toolbox based on its width (nice for dealing with multiple resolutions)
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
                Vector3 cropSelectionScale = cropSelection.localScale;
                cropSelection.DOScale(Vector3.zero,0.5f).OnComplete(()=> {cropSelection.localScale = cropSelectionScale;
                                                                                cropSelection.gameObject.SetActive(false);});
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