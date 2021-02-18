using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Prime31.MessageKit;

namespace FarmGame
{
    public class CropPanel : MonoBehaviour
    {
        [SerializeField]
        Crop associatedCrop;

        [SerializeField]
        TextMeshProUGUI cropNameText;

       [SerializeField]
        TextMeshProUGUI buyPriceText;

        [SerializeField]
        TextMeshProUGUI sellPriceText;

        [SerializeField]
        List<Image> cropSprites;
        [SerializeField]
        List<TextMeshProUGUI> cropTimes;


        private void Start() {
            SetCrop(associatedCrop);

            GetComponent<Button>().onClick.AddListener(() => MessageKit<Crop>.post(Messages.CropSet, associatedCrop));
        }

        void SetCrop(Crop c){
            associatedCrop = c;
            cropNameText.text = c.name;

            buyPriceText.text = "Buy : "+c.buyPrice+"$";
            sellPriceText.text = "Sell : "+c.sellPrice+"$";

            for(int i=0;i<c.sprites.Length;i++){
                cropSprites[i].sprite = c.sprites[i];
            }

            for(int i=0;i<c.growTime.Length;i++){
                cropTimes[i].text = c.growTime[i]+"h";
            }
        }
    }

}