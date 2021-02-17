using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame
{
    public class GlobalVariables : MonoBehaviour
    {
        [SerializeField]
        Sprite cropTilePrepareSprite;

        [SerializeField]
        List<Crop> availableCrops;

        public static Dictionary<int, Crop> cropsDictionary;
        
        public static int nightStart = 20;
        public static int nightEnd = 8;

        private void Awake() {
            CropTile.prepareSprite = cropTilePrepareSprite;

            cropsDictionary = new Dictionary<int, Crop>();
            for(int i=0;i<availableCrops.Count;i++){
                cropsDictionary.Add(availableCrops[i].cropID, availableCrops[i]);
            }
        }
    }
    public class Messages
    {
        public const int SwitchToField = 0;
        public const int SwitchToTopView = 1;
        public const int TryBuyField = 2;
        public const int GameTick = 3;
    }
}