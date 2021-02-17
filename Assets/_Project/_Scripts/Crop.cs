using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame
{
    [CreateAssetMenu(fileName = "New Crop", menuName = "ScriptableObjects/Crop")]
    public class Crop : ScriptableObject
    {
        public int cropID;
        public new string name;
        public int[] growTime = { 24, 48, 72, 96 };
        public Sprite[] sprites = { null, null, null, null };
        public int currentTime = 0;
        public int buyPrice = 0;
        public int sellPrice = 0;

    }

}