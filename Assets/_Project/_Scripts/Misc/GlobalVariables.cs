using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace FarmGame
{
    public class GlobalVariables : MonoBehaviour
    {
        [SerializeField]
        Sprite cropTilePrepareSprite;

        [SerializeField]
        List<Crop> availableCrops;

        [SerializeField]
        TMP_FontAsset font;

        public static TMP_FontAsset gameFont;
        public static Dictionary<int, Crop> cropsDictionary;
        
        public static int nightStart = 20;
        public static int nightEnd = 8;

        private void Awake() {
            CropTile.prepareSprite = cropTilePrepareSprite;

            cropsDictionary = new Dictionary<int, Crop>();
            for(int i=0;i<availableCrops.Count;i++){
                cropsDictionary.Add(availableCrops[i].cropID, availableCrops[i]);
            }

            gameFont = font;
        }
    }

    public class Messages
    {
        public const int SwitchToFieldView = 0;
        public const int SwitchToTopView = 1;
        public const int TryBuyField = 2;
        public const int GameTick = 3;
        public const int MoneyUpdate = 4;
        public const int SwitchAction = 5;
        public const int SwitchView = 6;
        public const int CropSet = 7;
        public const int NightSwitch = 8;
        public const int NewData = 9;
        public const int PositiveEvent = 10;
        public const int CropPlanted = 11;
        public const int ResetPressed = 12;
    }

    public enum Action { Harvest, Prepare, Plant, Delete };
}