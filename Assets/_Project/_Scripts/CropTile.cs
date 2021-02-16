using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame
{
    public class CropTile : Tile
    {
        Crop currentCrop = null;
        int currentCropTime = 0;
        int currentCropStep = 0;

        Vector3Int position;

        
        //Return true if it needs to be updated
        public bool UpdateTime()
        {
            //Shouldn't be possible
            if (currentCrop == null)
            {
                return false;
            }

            currentCropTime++;
            if (currentCropStep < 3 && currentCropTime >= currentCrop.growTime[currentCropStep])
            {
                currentCropStep++;
                this.sprite = currentCrop.sprites[currentCropStep];

                return true;
            }

            return false;
        }

        public void Init(Vector3Int pos, Crop crop){
            position = pos;
            currentCrop = crop;
            this.sprite = currentCrop.sprites[0];
        }

        public Vector3Int GetPosition(){
            return position;
        }

        public Crop GetCurrentCrop(){
            return currentCrop;
        }

        public bool CanHarvest()
        {
            return currentCropStep >= 2;
        }

        public int GetSellWorth(){
            return(currentCropStep == 2)?currentCrop.sellPrice:0;
        }
    }

}