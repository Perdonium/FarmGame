using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Prime31.MessageKit;

namespace FarmGame
{
    public class GameManager : MonoBehaviour
    {

        Camera mainCam;
        const float cameraMovementSpeed = 0.2f;
        const float gameTickTime = 1f; //Time in second for a game tick
        float gameTime = 0;


        List<CropTile> crops = new List<CropTile>();

        int playerMoney = 50;

        [SerializeField]
        Tilemap objectsTilemap;

        [SerializeField]
        Tilemap cropSpaceTilemap;

        [SerializeField]
        Crop plantA;
        [SerializeField]
        Crop plantB;

        Crop currentCrop;

        bool pause;
        bool topView = true; //The game starts on top view

        Vector3 lastMouseDownPosition = Vector3.zero;
        bool mouseClick = false;

        private void Awake()
        {
            mainCam = Camera.main;

            currentCrop = plantA;
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(GameTimeCoroutine());

            MessageKit.addObserver(Messages.SwitchToTopView, () => topView = true);
            MessageKit.addObserver(Messages.SwitchToField, () => topView = false);

            MessageKit<FarmField>.addObserver(Messages.TryBuyField, OnTryBuyField);
        }

        private void Update()
        {
            if (!topView)
            {
                //Mouse down
                if (Input.GetMouseButtonDown(0))
                {
                    lastMouseDownPosition = Input.mousePosition;
                    mouseClick = true;
                }

                //Mouse click
                if (Input.GetMouseButtonUp(0) && mouseClick)
                {
                    Vector3 mouseScreenPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
                    Vector3Int mouseCoords = objectsTilemap.WorldToCell(mouseScreenPosition);

                    if (cropSpaceTilemap.HasTile(mouseCoords))
                    {
                        //TODO : switch to another tilemap for crops
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

                    lastMouseDownPosition = Vector3.zero;
                }

                //Mouse drag
                if(Input.GetMouseButton(0) && lastMouseDownPosition != Input.mousePosition){
                    mouseClick = false;

                    
                    Vector3 dragDirection = lastMouseDownPosition - Input.mousePosition;
                    mainCam.transform.position += dragDirection.normalized * cameraMovementSpeed;


                    lastMouseDownPosition = Input.mousePosition;

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

        void OnTryBuyField(FarmField field)
        {
            if (field.GetCost() > playerMoney)
            {
                Debug.Log("Not enough money to buy field");
                return;
            }
            playerMoney -= field.GetCost();


            Tilemap fieldMap = field.GetFieldTilemap();
            BoundsInt fieldBounds = fieldMap.cellBounds;
            Vector3Int vec = Vector3Int.zero;
            for (int i = fieldBounds.xMin; i < fieldBounds.xMax; i++)
            {
                vec.x = i;
                for (int j = fieldBounds.yMin; j < fieldBounds.yMax; j++)
                {
                    vec.y = j;
                    if (fieldMap.HasTile(vec))
                    {
                        cropSpaceTilemap.SetTile(vec, fieldMap.GetTile(vec));
                    }
                }
            }

            field.SetBought(true);
        }

        public void Plant(Vector3Int position)
        {
            if (currentCrop == null)
            {
                return;
            }

            if (playerMoney < currentCrop.purchasePrice)
            {
                Debug.Log("Not enough money !");
                return;
            }

            Debug.Log("Plant");

            playerMoney -= currentCrop.purchasePrice;

            //TODO : optimize by object pooling ?
            CropTile newCrop = ScriptableObject.CreateInstance<CropTile>();
            newCrop.Init(position, currentCrop);
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
            pause = true;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            pause = false;
        }

        //For quick debug purpose only
        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 100, 20), "Money : " + playerMoney);

            if (pause)
            {
                if (GUI.Button(new Rect(10, 50, 100, 20), "Resume"))
                {
                    ResumeGame();
                }
            }
            else
            {
                if (GUI.Button(new Rect(10, 50, 100, 20), "Pause"))
                {
                    PauseGame();
                }
            }

            if (GUI.Button(new Rect(10, 80, 100, 20), "PlantA"))
            {
                currentCrop = plantA;
            }


            if (GUI.Button(new Rect(10, 110, 100, 20), "PlantB"))
            {
                currentCrop = plantB;
            }

            if (GUI.Button(new Rect(10, 140, 100, 20), "TopView"))
            {
                if (topView)
                {
                    MessageKit.post(Messages.SwitchToField);
                }
                else
                {
                    MessageKit.post(Messages.SwitchToTopView);
                }
            }
        }
    }

}