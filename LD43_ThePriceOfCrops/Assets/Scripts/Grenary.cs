﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenary : MonoBehaviour
{
    public static Grenary inst;

    #region Vars
    private StoredFood[] _foodStored = new StoredFood[3];
    #endregion
    #region MonoFunctions
    private void Awake()
    {
        inst = this;
    }
    private void Start()
    {
        CropsData[] crops = FoodManager.inst.GetCropsData();

        for (int i = 0; i < crops.Length; i++)
        {
            _foodStored[i] = new StoredFood(crops[i].id, crops[i].name, crops[i].food);
        }
    }
    private void Update()
    {

    }
    #endregion
    #region Functions
    public void AddFood(Crops food)
    {
        int id = food.GetCropId();
        _foodStored[id].quantity++;
    }
    public int FindBestFood()
    {
        int id = 0; //id of the better food in the grenary

        for(int i = 1; i < _foodStored.Length; i++)
        {
            if (_foodStored[i].quantity > 0 && _foodStored[i].foodValueByPart > _foodStored[id].foodValueByPart 
                && _foodStored[i].reservedQuantity < _foodStored[i].quantity)
                id = i;
        }
        if (id > 0)
            _foodStored[id].reservedQuantity++;
        else id = -1;

        return id;
    }
    public int GetClaimedFood(int id)
    {
        _foodStored[id].quantity--;
        _foodStored[id].reservedQuantity--;

        return _foodStored[id].foodValueByPart;
    }
    #endregion

    [System.Serializable]
    public struct StoredFood
    {
        public int id;
        public string foodName;
        public int quantity;
        public int foodValueByPart;
        public int reservedQuantity;

        public StoredFood(int id, string foodName, int foodValueByPart)
        {
            this.id = id;
            this.foodName = foodName;
            quantity = 0;
            this.foodValueByPart = foodValueByPart;
            reservedQuantity = 0;
        }
    }
}