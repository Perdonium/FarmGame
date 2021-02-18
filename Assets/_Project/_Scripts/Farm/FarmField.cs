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
    public class FarmField : MonoBehaviour
    {
        [SerializeField]
        Button fieldButton = null;

        [SerializeField]
        int cost = 0;

        [SerializeField]
        Tilemap fieldCropSpaceTilemap = null;

        public bool bought = false;

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