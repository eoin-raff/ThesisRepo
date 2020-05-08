using MED10.PCG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendToGoogle : MonoBehaviour
{
    public void SendToForms()
    {
        int seed = TerrainManager.Instance.GetTerrainGenerator().Seed;       // Get reference to seed here

        string seedString = seed.ToString();

        if (TerrainManager.Instance.GetTerrainGenerator().seedType == TerrainGenerator.SeedType.Fixed)
        {
            Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLScys2WbSD_cEcWUDwip8UyAzHrvdOk4AHFwhibrr8sDPqjn_Q/viewform?usp=pp_url&entry.594058135=Condition+A&entry.150889762=" + seedString);

        }
        else
        {
            Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLScys2WbSD_cEcWUDwip8UyAzHrvdOk4AHFwhibrr8sDPqjn_Q/viewform?usp=pp_url&entry.594058135=Condition+B&entry.150889762=" + seedString);
        }
    }
}
