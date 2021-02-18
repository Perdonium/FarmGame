using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Prime31.MessageKit;
using CodeMonkey.Utils;
using UnityEditor;
using DG.Tweening;

namespace FarmGame
{
    public class GameManager : MonoBehaviour
    {

        #region Properties

        Camera mainCam;
        float cameraMovementSpeed = 0.6f;

        [SerializeField]
        float gameTickTime = 1f; //Time in second for a game tick
        double gameTime = 0;

        List<CropTile> crops = new List<CropTile>();
        [SerializeField]
        List<FarmField> fields;

        [SerializeField]
        Tilemap cropsTilemap;
        [SerializeField]
        Tilemap cropsAvailableTilemap;
        Crop currentCrop;


        [SerializeField]
        int playerMoney = 50;

        bool topView = true; //The game starts on top view


        Vector3 lastMouseDownPosition = Vector3.zero;
        bool mouseClick = false;
        Action currentAction;
        bool onMobile = false;


        bool askedReset = false;

        #endregion

        private void Awake()
        {
            mainCam = Camera.main;

            //Platform dependant compilation is better for performances
            #if UNITY_EDITOR
                onMobile = false;
            #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
                onMobile = true;
                cameraMovementSpeed = 1f;
            #endif

            Debug.Log("On mobile : " + onMobile);
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(GameTimeCoroutine());

            MessageKit.addObserver(Messages.SwitchView, () => OnSwitchView());
            MessageKit.addObserver(Messages.SwitchToTopView, () => topView = true);
            MessageKit.addObserver(Messages.SwitchToFieldView, () => topView = false);
            MessageKit<FarmField>.addObserver(Messages.TryBuyField, OnTryBuyField);
            MessageKit<Action>.addObserver(Messages.SwitchAction, (a) => SwitchAction(a));
            MessageKit<Crop>.addObserver(Messages.CropSet, (c) => SetCurrentCrop(c));
            MessageKit.addObserver(Messages.ResetPressed, () => AskReset());
            MessageKit<CropTile>.addObserver(Messages.PreparedDecay, (ct) => Delete(ct));

            MessageKit<int>.post(Messages.MoneyUpdate, playerMoney);

        }

        //Update() is used for controls
        private void Update()
        {
            if (!topView)
            {

                if (!onMobile)
                {

                    //PC

                    //Mouse down
                    if (Input.GetMouseButtonDown(0))
                    {
                        lastMouseDownPosition = Input.mousePosition;
                        mouseClick = true;
                    }

                    //Mouse click
                    if (Input.GetMouseButtonUp(0) && mouseClick)
                    {

                        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
                        Vector3Int mouseCoords = cropsTilemap.WorldToCell(mouseWorldPosition);
                        ManageClick(mouseCoords);

                        lastMouseDownPosition = Vector3.zero;
                    }

                    //Mouse drag
                    if (Input.GetMouseButton(0) && lastMouseDownPosition != Input.mousePosition)
                    {
                        mouseClick = false;


                        Vector3 dragDirection = lastMouseDownPosition - Input.mousePosition;
                        mainCam.transform.position += dragDirection.normalized * cameraMovementSpeed;
                        MessageKit.post(Messages.CameraMoved);

                        lastMouseDownPosition = Input.mousePosition;

                    }

                }
                else
                {

                    //Phone

                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.touches[0];

                        if (touch.phase == TouchPhase.Began)
                        {
                            lastMouseDownPosition = Input.mousePosition;
                            mouseClick = true;
                        }
                        //Click
                        else if (touch.phase == TouchPhase.Ended && mouseClick)
                        {
                            Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(touch.position);
                            Vector3Int mouseCoords = cropsTilemap.WorldToCell(mouseWorldPosition);

                            ManageClick(mouseCoords);

                            lastMouseDownPosition = Vector3.zero;
                        }
                        //Drag
                        else if (touch.phase == TouchPhase.Moved)
                        {
                            //A little margin to help big fingers like mine
                            float diff = Vector2.Distance(lastMouseDownPosition, touch.position);
                            if (diff > 5f)
                            {
                                mouseClick = false;

                                Vector3 dragDirection = lastMouseDownPosition - (Vector3)touch.position;
                                mainCam.transform.position += dragDirection.normalized * cameraMovementSpeed;
                                MessageKit.post(Messages.CameraMoved);

                                lastMouseDownPosition = touch.position;
                            }
                        }
                    }

                }

            }

        }

        //Manage a player's click based on its position
        void ManageClick(Vector3Int mouseCoordinates)
        {

            //I set the informations I need as variables early just to prevent getting them in each if
            bool tileAlreadyPlaced = cropsTilemap.HasTile(mouseCoordinates);
            CropTile mouseTile = null;
            if (tileAlreadyPlaced)
            {
                mouseTile = (CropTile)(cropsTilemap.GetTile(mouseCoordinates));
            }

            if (currentAction == Action.Prepare && cropsAvailableTilemap.HasTile(mouseCoordinates) && !tileAlreadyPlaced)
            {
                Prepare(mouseCoordinates);
                MessageKit.post(Messages.NewData);
            }
            else if (currentAction == Action.Delete && tileAlreadyPlaced)
            {
                Delete(mouseTile);
                MessageKit.post(Messages.NewData);
            }
            else if (currentAction == Action.Plant && tileAlreadyPlaced)
            {
                if (mouseTile.GetCurrentCrop() == null)
                {
                    Plant(mouseTile);
                    MessageKit.post(Messages.NewData);
                }
            }
            else if (currentAction == Action.Harvest && tileAlreadyPlaced)
            {
                if (mouseTile.CanHarvest())
                {
                    Harvest(mouseTile);
                    MessageKit.post(Messages.NewData);
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

            MessageKit<double>.post(Messages.GameTick, gameTime);

            //Update all the crops's times
            //It's either that or a coroutine on each crop
            for (int i = 0; i < crops.Count; i++)
            {

                if (crops[i].UpdateTime())
                {
                    cropsTilemap.RefreshTile(crops[i].GetPosition());
                }
            }


            double currentHour = (((gameTime % 24) + 8) % 24);

            if (currentHour == GlobalVariables.nightStart)
            {
                MessageKit<bool>.post(Messages.NightSwitch, true);
            }
            else if (currentHour == GlobalVariables.nightEnd)
            {
                MessageKit<bool>.post(Messages.NightSwitch, false);

                //Just a little help for the player in case they get stuck
                playerMoney += 100;
                MessageKit<int>.post(Messages.MoneyUpdate, playerMoney);
            }
        }

        void OnSwitchView()
        {
            if (topView)
            {
                MessageKit.post(Messages.SwitchToFieldView);
            }
            else
            {
                MessageKit.post(Messages.SwitchToTopView);
                lastMouseDownPosition = Vector3.zero;
            }
        }

        void SwitchAction(Action a)
        {
            currentAction = a;
        }

        void OnTryBuyField(FarmField field)
        {
            if (field.GetCost() > playerMoney)
            {
                Debug.Log("Not enough money to buy field");
                UtilsClass.CreateWorldTextPopup("Not enough money", (lastMouseDownPosition == Vector3.zero)?mainCam.transform.position:mainCam.ScreenToWorldPoint(lastMouseDownPosition));
                return;
            }

            playerMoney -= field.GetCost();
            MessageKit<int>.post(Messages.MoneyUpdate, playerMoney);
            UtilsClass.CreateWorldTextPopup("Field bought !", field.transform.position);
            MessageKit.post(Messages.PositiveEvent);

            BuyField(field);
        }

        public void BuyField(FarmField field)
        {

            //Merge the field tilemap with the cropsAvailableTilemap
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
                        cropsAvailableTilemap.SetTile(vec, fieldMap.GetTile(vec));
                    }
                }
            }

            field.SetBought(true);
        }

        public void Prepare(Vector3Int position)
        {
            CropTile newCrop = ScriptableObject.CreateInstance<CropTile>();
            newCrop.SetPosition(position);
            cropsTilemap.SetTile(position, newCrop);

            crops.Add(newCrop);
        }

        public void Plant(CropTile tile)
        {
            if (playerMoney < currentCrop.buyPrice)
            {
                Debug.Log("Not enough money !");
                UtilsClass.CreateWorldTextPopup("Not enough money", mainCam.ScreenToWorldPoint(lastMouseDownPosition));
                return;
            }

            playerMoney -= currentCrop.buyPrice;
            MessageKit<int>.post(Messages.MoneyUpdate, playerMoney);
            UtilsClass.CreateWorldTextPopup("-" + currentCrop.buyPrice + "$", mainCam.ScreenToWorldPoint(lastMouseDownPosition));

            tile.SetCrop(currentCrop);
            cropsTilemap.RefreshTile(tile.GetPosition());
        }

        public void Harvest(CropTile tile)
        {
            if (!tile.CanHarvest())
            {
                Debug.Log("Can't harvest yet");
                return;
            }

            int sellWorth = tile.GetSellWorth();
            playerMoney += sellWorth;
            MessageKit<int>.post(Messages.MoneyUpdate, playerMoney);
            UtilsClass.CreateWorldTextPopup("+" + sellWorth + "$", mainCam.ScreenToWorldPoint(lastMouseDownPosition));
            MessageKit.post(Messages.PositiveEvent);

            cropsTilemap.SetTile(tile.GetPosition(), null);
            crops.Remove(tile);
            GameObject.Destroy(tile);
        }

        public void Delete(CropTile tile)
        {
            cropsTilemap.SetTile(tile.GetPosition(), null);
            crops.Remove(tile);
            GameObject.Destroy(tile);
        }

        #region GetSet

        public List<CropTile> GetCrops()
        {
            return crops;
        }

        public void SetCrops(List<CropTile> c)
        {
            crops.Clear();
            crops = c;
            for (int i = 0; i < crops.Count; i++)
            {
                cropsTilemap.SetTile(crops[i].GetPosition(), crops[i]);
                cropsTilemap.RefreshTile(crops[i].GetPosition());
            }
        }

        void SetCurrentCrop(Crop c)
        {
            currentCrop = c;
            MessageKit<Action>.post(Messages.SwitchAction, Action.Plant);
        }

        public List<FarmField> GetFields()
        {
            return fields;
        }

        public int GetMoney()
        {
            return playerMoney;
        }

        public double GetGameTime()
        {
            return gameTime;
        }

        public void SetMoney(int money)
        {
            playerMoney = money;
            MessageKit<int>.post(Messages.MoneyUpdate, playerMoney);
        }

        public void SetGameTime(double time)
        {
            gameTime = time;
            MessageKit<double>.post(Messages.GameTick, gameTime);
            if (gameTime == GlobalVariables.nightStart)
            {
                MessageKit<bool>.post(Messages.NightSwitch, true);
            }
            else if (gameTime == GlobalVariables.nightEnd)
            {
                MessageKit<bool>.post(Messages.NightSwitch, false);
            }
        }

        #endregion

        public void AskReset()
        {
            if(askedReset){
                askedReset = false;
                Reset();
                UtilsClass.CreateWorldTextPopup("Data reset", mainCam.transform.position);
                return;
            }

            UtilsClass.CreateWorldTextPopup("Press again if you want to reset your data", mainCam.transform.position);
            askedReset = true;
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.2f);
            seq.AppendCallback(() => askedReset = false);
        }

        public void Reset()
        {
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].SetBought(false);
            }

            cropsAvailableTilemap.ClearAllTiles();
            cropsTilemap.ClearAllTiles();

            crops.Clear();
            gameTime = gameTime % 24;
            playerMoney = 50;

            MessageKit<int>.post(Messages.MoneyUpdate, playerMoney);
            MessageKit.post(Messages.NewData);
            MessageKit<double>.post(Messages.GameTick, gameTime);

        }
    }

}