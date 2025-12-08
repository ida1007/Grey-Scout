using System.Collections.Generic;
using UnityEngine;

public class HostageManager : MonoBehaviour
{
    public static HostageManager Instance;
    public List<GameObject> followHostages = new List<GameObject>();
    public List<GameObject> boatHostages = new List<GameObject>();

    public int hostageNum = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Awake()
    {
        Instance = this;
    }
    public void AddRescuedHostage(GameObject hostage)
    {
        hostageNum++;
        followHostages.Add(hostage);
    }
    public void MoveHostage()
    {
        foreach (var h in followHostages)
        {
            if (h != null)
                Destroy(h);
        }
        followHostages.Clear();

        for (int i = 0; i < boatHostages.Count; i++)
        {
            GameObject boatHostage = boatHostages[i];

            if (boatHostage == null)
                continue;
            if (i < hostageNum)
                boatHostage.SetActive(true);
            else
                boatHostage.SetActive(false);

        }
    }
}

