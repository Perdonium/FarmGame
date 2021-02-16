using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager INSTANCE;

        Camera mainCam;

        const float gameTickTime = 1f; //Time in second for a game tick
        float gameTime = 0;


        List<CropTile> crops = new List<CropTile>();

        int playerMoney = 50;

        [SerializeField]
        Tilemap objectsTilemap;

        [SerializeField]
        Crop defaultCrop;

        private void Awake()
        {
            INSTANCE = this;
            mainCam = Camera.main;
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(GameTimeCoroutine());
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int mouseCoords = (Vector3Int)objectsTilemap.WorldToCell(mousePos);

                if (!objectsTilemap.HasTile(mouseCoords))
                {
                    Plant(mouseCoords);
                }
                else
                {
                    TileBase mouseTile = objectsTilemap.GetTile(mouseCoords);
                    if (mouseTile is CropTile)
                    {
                        Harvest((CropTile)mouseTile);
                    }
                }
            }
        }

        //Update the game time
        IEnumerator GameTimeCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(gameTickTime);

                gameTime += 1;
                OnGameTick();
            }
        }

        void OnGameTick()
        {

            //Update all the crops's times
            //It's either that or a coroutine on each crop
            int cropsCount = crops.Count;
            for (int i = 0; i < cropsCount; i++)
            {

                if (crops[i].UpdateTime())
                {
                    objectsTilemap.RefreshTile(crops[i].GetPosition());
                }
            }
        }

        public void Plant(Vector3Int position)
        {
            if (playerMoney < defaultCrop.purchasePrice)
            {
                Debug.Log("Not enough money !");
                return;
            }

            Debug.Log("Plant");

            playerMoney -= defaultCrop.purchasePrice;

            //TODO : optimize by object pooling ?
            CropTile newCrop = ScriptableObject.CreateInstance<CropTile>();
            newCrop.Init(position, defaultCrop);
            objectsTilemap.SetTile(position, newCrop);

            crops.Add(newCrop);
        }

        public void Harvest(CropTile tile)
        {
            if (!tile.CanHarvest())
            {
                Debug.Log("Can't harvest yet");
                return;
            }
            playerMoney += tile.GetSellWorth();

            objectsTilemap.SetTile(tile.GetPosition(), null);
            crops.Remove(tile);
            GameObject.Destroy(tile);
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }


        //For quick debug purpose only
        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 100, 20), "Money : "+playerMoney);
        }
    }

}