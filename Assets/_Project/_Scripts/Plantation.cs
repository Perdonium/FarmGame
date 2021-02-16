using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame
{
    public class Plantation : MonoBehaviour
    {
        [SerializeField]
        SpriteRenderer spriteRenderer;

        [SerializeField]
        Crop defaultCrop;

        Crop currentCrop = null;
        int currentCropTime = 0;
        int currentCropStep = 0;

        public void UpdateTime()
        {
            if (currentCrop == null) return;

            currentCropTime++;
            if (currentCropStep < 3 && currentCropTime >= currentCrop.growTime[currentCropStep])
            {
                currentCropStep++;
                spriteRenderer.sprite = currentCrop.sprites[currentCropStep];
            }
        }

        private void OnMouseDown()
        {
            if (currentCrop == null)
            {
                Plant(defaultCrop);
            }
            else
            {
                Harvest();
            }
        }
        public void Plant(Crop crop)
        {
            currentCrop = crop;
            currentCropTime = 0;
            currentCropStep = 0;
            spriteRenderer.sprite = currentCrop.sprites[currentCropStep];

            //GameManager.INSTANCE.OnPlant(crop);
        }

        public void Harvest()
        {
            if (currentCropStep == 2)
            {
                //GameManager.INSTANCE.OnHarvest(currentCrop);
            }

            currentCrop = null;
            spriteRenderer.sprite = null;
        }
    }

}