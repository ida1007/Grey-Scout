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

        // 可选：初始化时全关
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

        // 这次搬运后船上应有的“总数”
        int tempTotalNum = currentBoatNum + followHostages.Count;
        tempTotalNum = Mathf.Clamp(tempTotalNum, 0, boatHostages.Count);

        // 销毁跟随人质（你原来的逻辑）
        foreach (var h in followHostages)
        {
            if (h != null)
                Destroy(h);
        }

        // 根据 tempTotalNum 显示船上的“站位人质”
        for (int i = 0; i < boatHostages.Count; i++)
        {
            GameObject boatHostage = boatHostages[i];
            if (boatHostage == null) continue;

            boatHostage.SetActive(i < tempTotalNum);
        }

        // 键：别在循环里 ++，直接赋值
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
