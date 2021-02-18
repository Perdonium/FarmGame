using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using Prime31.MessageKit;
using UnityEngine.Tilemaps;
using TMPro;

namespace FarmGame
{
    //A field buyable by the player
    public class FarmField : MonoBehaviour
    {

        #region Properties

        [SerializeField]
        Button fieldButton = null;

        [SerializeField]
        int cost = 0;

        [SerializeField]
        Tilemap fieldCropSpaceTilemap = null; //The tilemap of crop region it unlocks when bought

        public bool bought = false;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            fieldButton.onClick.AddListener(() => OnButtonClick());
            fieldButton.transform.GetComponentInChildren<TextMeshProUGUI>(true).text = cost+"$";
        }

        void OnButtonClick()
        {
            MessageKit<FarmField>.post(Messages.TryBuyField, this);
        }

        public int GetCost(){
            return cost;
        }

        public Tilemap GetFieldTilemap(){
            return fieldCropSpaceTilemap;
        }

        public void SetBought(bool b){
            bought = b;

            if(b){
                fieldButton.gameObject.SetActive(false);
            } else {
                fieldButton.gameObject.SetActive(true);
            }
        }
    }

}