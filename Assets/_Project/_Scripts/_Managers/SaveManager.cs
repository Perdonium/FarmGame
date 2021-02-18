﻿using System.Runtime.Serialization.Formatters;
using System.Diagnostics.Contracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Prime31.MessageKit;

namespace FarmGame
{
    public class SaveManager : MonoBehaviour
    {
        string filename = "savefile";
        string saveFilePath = "";

        GameManager gameManager;

        bool onMobile = false;
        private void Awake()
        {
            //Platform dependant compilation is better for performances
            #if UNITY_EDITOR
                onMobile = false;
            #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
                onMobile = true;
            #endif

            if (onMobile)
            {
                saveFilePath = Application.persistentDataPath + "/" + filename + ".txt";
            }
            else
            {
                saveFilePath = Application.dataPath + "/" + filename + ".txt";
            }

            gameManager = GetComponent<GameManager>();
            Load();

            MessageKit<double>.addObserver(Messages.GameTick, (d) => Save());
            MessageKit.addObserver(Messages.NewData, () => Save());
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


            File.WriteAllText(saveFilePath, jsonData);
        }

        void Load()
        {
            if (File.Exists(saveFilePath))
            {

                string saveString = File.ReadAllText(saveFilePath);

                GameData savedData = JsonUtility.FromJson<GameData>(saveString);

                gameManager.SetMoney(savedData.money);
                gameManager.SetGameTime(savedData.gameTime);

                List<FarmField> fields = gameManager.GetFields();
                for (int i = 0; i < savedData.boughtFields.Count; i++)
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
            }
            else
            {
                Debug.Log("Nothing to load");
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