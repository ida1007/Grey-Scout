using System.Collections.Generic;
using UnityEngine;

public class HostageManager : MonoBehaviour
{
    public Transform player;
    public static HostageManager Instance;
    private List<GameObject> followHostages = new List<GameObject>();
    private List<GameObject> boatHostages = new List<GameObject>();
    
    private int currentBoatNum;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Awake()
    {
        Instance = this;
    }

    public void InitializeBoatHostages(List<GameObject> boatHostagesLine)
    {
        boatHostages = boatHostagesLine;
    }

    public void AddRescuedHostage(GameObject hostage)
    {
        followHostages.Add(hostage);
    }

    public bool HasSameHostages(GameObject gameObject)
    {
        return followHostages.Contains(gameObject);

    }

    public void MoveHostage()
    {
        int tempTotalNum = currentBoatNum + followHostages.Count;

        foreach (var h in followHostages)
        {
            if (h != null)
                Destroy(h);
        }
        
        for (int i = 0; i < boatHostages.Count; i++)
        {
            GameObject boatHostage = boatHostages[i];

            if (boatHostage == null)
                continue;
            if (i < tempTotalNum)
            {
                boatHostage.SetActive(true);
                currentBoatNum++;
            }

            else
                boatHostage.SetActive(false);

        }
        followHostages.Clear();
    }
    public Transform GetLastHostage()
    {
        if(followHostages == null)
            return null;
        if(followHostages.Count == 0)
            return player;
        return followHostages[followHostages.Count-1].transform;
    }
}

