using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Northwind.Shaders.InteractiveVegetation
{
    [ExecuteInEditMode]
    [AddComponentMenu("Northwind/Interactive Vegetation/Burn Starter")]
    public class BurnStarter : MonoBehaviour
    {

        public bool globalBurn = false;
        public bool reset = false;

        // Use this for initialization
        void Start()
        {
            Shader.SetGlobalFloat("_BurnTime", float.MaxValue);
        }

        // Update is called once per frame
        void Update()
        {
            if (globalBurn)
            {
                StartGlobalBurn();
                globalBurn = false;
            }

            if (reset)
            {
                ResetBurn();
                reset = false;
            }
        }

        public void StartGlobalBurn()
        {
            Shader.SetGlobalFloat("_BurnTime", Time.time);
            Debug.Log("<color=cyan>DDL_Vegetation: </color>Burning started at " + Time.time);
        }

        public void ResetBurn()
        {
            Shader.SetGlobalFloat("_BurnTime", float.MaxValue);
            Debug.Log("<color=cyan>DDL_Vegetation: </color>Burning reseted");
        }
    }
}