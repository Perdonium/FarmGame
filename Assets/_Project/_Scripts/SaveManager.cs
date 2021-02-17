using System.Runtime.Serialization.Formatters;
using System.Diagnostics.Contracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace FarmGame
{
    public class SaveManager : MonoBehaviour
    {
        string filename = "savefile";

        GameManager gameManager;
        private void Awake()
        {
            gameManager = GetComponent<GameManager>();
        }


        void Save()
        {
            GameData gd = new GameData();
            gd.money = gameManager.GetMoney();
            gd.gameTime = gameManager.GetGameTime();

            //Saving bought fields
            List<FarmField> fields = gameManager.GetFields();
            List<int> boughtFields = new List<int>();

            for (int i = 0; i < fields.Count; i++)
            {
                boughtFields.Add((fields[i].bought) ? 1 : 0);
            }

            gd.boughtFields = boughtFields;

            //Saving crops
            List<CropTile> crops = gameManager.GetCrops();
            List<CropData> cropsData = new List<CropData>();
            CropData currentCrop;

            for (int i = 0; i < crops.Count; i++)
            {
                currentCrop = new CropData();
                currentCrop.position = crops[i].GetPosition();
                currentCrop.cropTime = crops[i].GetCurrentCropTime();
                if (crops[i].GetCurrentCrop() != null)
                {
                    currentCrop.cropID = crops[i].GetCurrentCrop().cropID;
                    currentCrop.cropStep = crops[i].GetCurrentCropStep();
                }

                cropsData.Add(currentCrop);

            }

            gd.crops = cropsData;

            string jsonData = JsonUtility.ToJson(gd);

            File.WriteAllText(Application.dataPath + "/" + filename + ".txt", jsonData);

            Debug.Log("Saved");
        }

        void Load()
        {
            if (File.Exists(Application.dataPath + "/" + filename + ".txt"))
            {
                string saveString = File.ReadAllText(Application.dataPath + "/" + filename + ".txt");

                GameData savedData = JsonUtility.FromJson<GameData>(saveString);

                gameManager.SetMoney(savedData.money);
                gameManager.SetGameTime(savedData.gameTime);

                List<FarmField> fields = gameManager.GetFields();
                for (int i = 0; i < fields.Count; i++)
                {
                    if (savedData.boughtFields[i] == 1)
                    {
                        gameManager.BuyField(fields[i]);
                    }
                }

                List<CropTile> cropTiles = new List<CropTile>();
                for (int i = 0; i < savedData.crops.Count; i++)
                {
                    CropTile tile = ScriptableObject.CreateInstance<CropTile>();
                    tile.SetPosition(savedData.crops[i].position);
                    tile.SetCurrentCropTime(savedData.crops[i].cropTime);
                    if (savedData.crops[i].cropID != -1)
                    {
                        tile.SetCurrentCropStep(savedData.crops[i].cropStep);
                        tile.SetCurrentCrop(GlobalVariables.cropsDictionary[savedData.crops[i].cropID]);
                    }

                    cropTiles.Add(tile);
                }

                gameManager.SetCrops(cropTiles);
            } else {
                Debug.Log("Nothing to load");
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
        }
    }


    [Serializable]
    public class CropData
    {
        public int cropID = -1;
        public Vector3Int position;
        public int cropTime = 0;
        public int cropStep = 0;

    }

    class GameData
    {
        public int money;
        public double gameTime;
        public List<int> boughtFields;
        public List<CropData> crops;
    }
}