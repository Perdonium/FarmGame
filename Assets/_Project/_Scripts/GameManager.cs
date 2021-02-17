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
        const float cameraMovementSpeed = 0.6f;
        const float gameTickTime = 1f; //Time in second for a game tick
        double gameTime = 0;


        List<CropTile> crops = new List<CropTile>();

        [SerializeField]
        List<FarmField> fields;

        int playerMoney = 50;

        [SerializeField]
        Tilemap cropsTilemap;

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

        Action currentAction;

        bool onMobile = false;

        private void Awake()
        {
            mainCam = Camera.main;

            currentCrop = plantA;

            //Platform dependant compilation is better for performances
            #if UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
                onMobile = true;
            #else
                onMobile = false;
            #endif
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(GameTimeCoroutine());

            MessageKit.addObserver(Messages.SwitchView, () => OnSwitchView());
            MessageKit.addObserver(Messages.SwitchToTopView, () => topView = true);
            MessageKit.addObserver(Messages.SwitchToField, () => topView = false);

            MessageKit<FarmField>.addObserver(Messages.TryBuyField, OnTryBuyField);

            MessageKit<int>.post(Messages.MoneyUpdate,playerMoney);

            MessageKit<Action>.addObserver(Messages.SwitchAction, (a) => SwitchAction(a));
            MessageKit<Crop>.addObserver(Messages.CropSet, (c) => SetCurrentCrop(c));
        }

        private void Update()
        {
            if (!topView)
            {
                if (!onMobile)
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
                        else if (touch.phase == TouchPhase.Ended && mouseClick)
                        {
                            Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(touch.position);
                            Vector3Int mouseCoords = cropsTilemap.WorldToCell(mouseWorldPosition);

                            ManageClick(mouseCoords);

                            lastMouseDownPosition = Vector3.zero;
                        }
                        else if (touch.phase == TouchPhase.Moved)
                        {
                            mouseClick = false;

                            Vector3 dragDirection = lastMouseDownPosition - (Vector3)touch.position;
                            mainCam.transform.position += dragDirection.normalized * cameraMovementSpeed;


                            lastMouseDownPosition = touch.position;
                        }
                    }

                }

            }

        }


        void ManageClick(Vector3Int mouseCoordinates)
        {
            bool tileAlreadyPlaced = cropsTilemap.HasTile(mouseCoordinates);
            CropTile mouseTile = null;
            if (tileAlreadyPlaced)
            {
                mouseTile = (CropTile)(cropsTilemap.GetTile(mouseCoordinates));
            }

            if (currentAction == Action.Prepare && cropSpaceTilemap.HasTile(mouseCoordinates) && !tileAlreadyPlaced)
            {
                Prepare(mouseCoordinates);
            }
            else if (currentAction == Action.Delete && tileAlreadyPlaced)
            {
                Delete(mouseTile);
            }
            else if (currentAction == Action.Plant && tileAlreadyPlaced)
            {
                if (mouseTile.GetCurrentCrop() == null)
                {
                    Plant(mouseTile);
                }
            }
            else if (currentAction == Action.Harvest && tileAlreadyPlaced)
            {
                if (mouseTile.CanHarvest())
                {
                    Harvest(mouseTile);
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
            int cropsCount = crops.Count;
            for (int i = 0; i < cropsCount; i++)
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
                MessageKit<bool>.post(Messages.NightSwitch, true);
            }
        }

        void OnSwitchView(){
            if(topView){
                MessageKit.post(Messages.SwitchToField);
            } else {
                MessageKit.post(Messages.SwitchToTopView);
            }
        }
        void SwitchAction(Action a){
            currentAction = a;
        }

        void OnTryBuyField(FarmField field)
        {
            if (field.GetCost() > playerMoney)
            {
                Debug.Log("Not enough money to buy field");
                return;
            }
            playerMoney -= field.GetCost();

            BuyField(field);


        }

        public void BuyField(FarmField field){
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
        public void Prepare(Vector3Int position)
        {
            //TODO : optimize by object pooling ?
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
                return;
            }

            playerMoney -= currentCrop.buyPrice;
            MessageKit<int>.post(Messages.MoneyUpdate,playerMoney);

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

            playerMoney += tile.GetSellWorth();
            MessageKit<int>.post(Messages.MoneyUpdate,playerMoney);

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

    
        public List<CropTile> GetCrops(){
            return crops;
        }

        public void SetCrops(List<CropTile> c){
            crops.Clear();
            crops = c;
            for(int i=0;i<crops.Count;i++){
                cropsTilemap.SetTile(crops[i].GetPosition(),crops[i]);
                cropsTilemap.RefreshTile(crops[i].GetPosition());
            }
        }

        void SetCurrentCrop(Crop c){
            currentCrop = c;
            MessageKit<Action>.post(Messages.SwitchAction, Action.Plant);
        }

        public List<FarmField> GetFields(){
            return fields;
        }

        public int GetMoney(){
            return playerMoney;
        }

        public double GetGameTime(){
            return gameTime;
        }
        
        public void SetMoney(int money){
            playerMoney = money;
            MessageKit<int>.post(Messages.MoneyUpdate,playerMoney);
        }

        public void SetGameTime(double time){
            gameTime = time;
            MessageKit<double>.post(Messages.GameTick,gameTime);
        }

        /*
        //For quick debug purpose only
        void OnGUI()
        {
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


            if (GUI.Button(new Rect(10, 170, 100, 20), "Prepare"))
            {
                currentAction = Action.Prepare;
            }

            if (GUI.Button(new Rect(10, 200, 100, 20), "Plant"))
            {
                currentAction = Action.Plant;
            }

            if (GUI.Button(new Rect(10, 230, 100, 20), "Delete"))
            {
                currentAction = Action.Delete;
            }

            if (GUI.Button(new Rect(10, 260, 100, 20), "None"))
            {
                currentAction = Action.Harvest;
            }
        }
        */
    }

}