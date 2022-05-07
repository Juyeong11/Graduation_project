using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyManager : MonoBehaviour
{
    public static PartyManager instance;
    public List<GameObject> partyObjects;
    public ClassImageObject CIO;
    public Dictionary<int, GameObject> party = new Dictionary<int, GameObject>();
    public static List<(int, int, string)> partyDatas = new List<(int, int, string)>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (party.Count == 0)
        {
            foreach (var d in partyDatas)
            {
                SetParty(d.Item1, d.Item2, d.Item3);
            }
        }
    }

    public void SetParty(int pid, int cid, string pName)
    {
        if (!party.ContainsKey(pid))
        {
            partyDatas.Add((pid, cid, pName));

            party.Add(pid, partyObjects[party.Count]);

            party[pid].GetComponent<Image>().sprite = CIO.ClassSprites[cid];
            party[pid].GetComponentInChildren<TMPro.TMP_Text>().text = pName;
            party[pid].SetActive(true);
        }

    }
    public void DelParty(int pid)
    {
        if (party.ContainsKey(pid))
        {
            partyDatas.Remove(partyDatas.Find(x => x.Item1 == pid));
            party[pid].SetActive(false);
            //Debug.LogError("!A " + party.Count);
            party.Remove(pid);
            //Debug.LogError("B! " + party.Count);
        }
    }
    public void DelParty()
    {
        foreach (var item in party)
        {
            item.Value.SetActive(false);
        }
        partyDatas.Clear();
        party.Clear();
    }

    public void PartyChangeClass(int pid, int cid)
    {
        if (party.ContainsKey(pid))
        {
            partyDatas.Add((pid, cid, partyDatas.Find(x => x.Item1 == pid).Item3));
            partyDatas.Remove(partyDatas.Find(x => x.Item1 == pid && x.Item2 != cid));
            party[pid].GetComponent<Image>().sprite = CIO.ClassSprites[cid];
        }
    }
}
