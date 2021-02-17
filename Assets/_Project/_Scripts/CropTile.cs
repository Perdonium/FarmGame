using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame
{
    public class CropTile : Tile
    {
        static int maxPrepareTime = 10;
        public static Sprite prepareSprite;

        Crop currentCrop = null;
        int currentCropTime = 0;
        int currentCropStep = 0;

        Vector3Int position;

        //Return true if it needs to be updated
        public bool UpdateTime()
        {

            currentCropTime++;
            if (currentCrop != null)
            {
                if (currentCropStep < 3 && currentCropTime >= currentCrop.growTime[currentCropStep])
                {
                    currentCropStep++;
                    this.sprite = currentCrop.sprites[currentCropStep];
                    return true;
                }
            } else {
                if(currentCropTime>maxPrepareTime){
                    //TODO : Remove
                }
            }
            return false;
        }

        public void SetPosition(Vector3Int pos)
        {
            position = pos;
            this.sprite = prepareSprite;

        }

        public void SetCrop(Crop crop)
        {
            currentCrop = crop;
            currentCropTime = 0;
            this.sprite = currentCrop.sprites[0];
        }

        public Vector3Int GetPosition()
        {
            return position;
        }

        public Crop GetCurrentCrop()
        {
            return currentCrop;
        }

        public int GetCurrentCropTime(){
            return currentCropTime;
        }

        public int GetCurrentCropStep(){
            return currentCropStep;
        }

        public void SetCurrentCrop(Crop c){
            currentCrop = c;
            this.sprite = currentCrop.sprites[currentCropStep];
        }

        public void SetCurrentCropTime(int time){
            currentCropTime = time;
        }

        public void SetCurrentCropStep(int step){
            currentCropStep = step;
        }
        public bool CanHarvest()
        {
            return currentCropStep >= 2;
        }

        public int GetSellWorth()
        {
            return (currentCropStep == 2) ? currentCrop.sellPrice : 0;
        }
    }

}