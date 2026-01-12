using System.Collections.Generic;
using UnityEngine;

public class HostageManager : MonoBehaviour
{
    public Transform player;
    public static HostageManager Instance;

    private List<GameObject> followHostages = new List<GameObject>();
    private List<GameObject> boatHostages = new List<GameObject>();

    private int currentBoatNum;

    public int CurrentBoatNum => currentBoatNum;
    public int BoatCapacity => boatHostages != null ? boatHostages.Count : 0;
    public bool IsBoatFull => BoatCapacity > 0 && currentBoatNum >= BoatCapacity;

    public void Awake()
    {
        Instance = this;
    }

    public void InitializeBoatHostages(List<GameObject> boatHostagesLine)
    {
        boatHostages = boatHostagesLine;

        // Close all at first
        for (int i = 0; i < boatHostages.Count; i++)
        {
            if (boatHostages[i] != null)
                boatHostages[i].SetActive(false);
        }

        currentBoatNum = 0;
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
        if (boatHostages == null || boatHostages.Count == 0)
            return;

       // Boat num this time should be
        int tempTotalNum = currentBoatNum + followHostages.Count;
        tempTotalNum = Mathf.Clamp(tempTotalNum, 0, boatHostages.Count);

        // Destroy Follow Hostage
        foreach (var h in followHostages)
        {
            if (h != null)
                Destroy(h);
        }

        // follow tempTotalNum show Hostage in Boat
        for (int i = 0; i < boatHostages.Count; i++)
        {
            GameObject boatHostage = boatHostages[i];
            if (boatHostage == null) continue;

            boatHostage.SetActive(i < tempTotalNum);
        }

        currentBoatNum = tempTotalNum;

        followHostages.Clear();
    }

    public Transform GetLastHostage()
    {
        if (followHostages == null)
            return null;
        if (followHostages.Count == 0)
            return player;
        return followHostages[followHostages.Count - 1].transform;
    }
}
