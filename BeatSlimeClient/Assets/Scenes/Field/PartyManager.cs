using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyManager : MonoBehaviour
{
    static public PartyManager instance;
    public List<GameObject> partyObjects;
    public List<Sprite> images;
    Dictionary<int,GameObject> party = new Dictionary<int,GameObject>();

    public void SetParty(int pid, int cid)
    {
        party.Add(pid, partyObjects[party.Count]);
        
        party[pid].GetComponent<Image>().sprite = images[cid];
        party[pid].GetComponentInChildren<TMPro.TMP_Text>().text = pid.ToString();
        party[pid].SetActive(true);

    }
    public void DelParty(int pid)
    {
        if (party.ContainsKey(pid))
        {
            partyObjects[pid].SetActive(false);
            party.Remove(pid);
        }
    }

    public void PartyChangeClass(int pid, int cid)
    {
        if (party.ContainsKey(pid))
        {
            party[pid].GetComponent<Image>().sprite = images[cid];
        }
    }
}
