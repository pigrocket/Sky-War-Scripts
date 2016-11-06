using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Respawn : NetworkBehaviour {
    
    [SyncVar]
    public int playerCount;
    
    [SyncVar]
    public string playerName;
    
    private static System.Random rand = new System.Random();
    
    //List<string> playerList = new List<string>();

	Rigidbody rigB;
	public GameObject spawnpoint;
	public GameObject[] shopPrefabs;
	bool respawn;
    
	// Use this for initialization
	void Start () {
        if (isServer) {
            playerName = RandomString(9);
            //playerList.Add(this.playerControllerId);
        }
        else {
            //CmdPlayerJoin(this.playerControllerId); //this will add this player object to server's array of players
		}
        
        if (!isLocalPlayer) return;
		respawn = false;
		spawnpoint = GameObject.Find("spawn");
		rigB = GetComponent<Rigidbody>();
        transform.position = spawnpoint.transform.position + new Vector3(0,2.5f,0);
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) return;
		if (rigB.position.y < -80) respawn = true;
		else respawn = false;
		if (respawn) transform.position = spawnpoint.transform.position + new Vector3(0,2.5f,0);
	}
	
    public void Buy (int i){
        CmdBuy(i, playerName);
    }
    
	[Command]
	void CmdBuy (int i, string p) {
		Vector3 v3 = new Vector3(0,0,0);
		var merch = (GameObject)Instantiate(shopPrefabs[i], v3, Quaternion.identity, gameObject.transform);
		NetworkServer.SpawnWithClientAuthority(merch, connectionToClient);
        merch.GetComponent<Pickupable>().PostStart(p);
	}
    
    public static string RandomString(int length)
    {
        string chars = "AESTHETIC";
        var stringchars = new char[length];
        for (int i = 0; i < stringchars.Length; i++)
        {
            stringchars[i] = chars[rand.Next(chars.Length)];
        }
        return new string(stringchars);
    }
    
    public Transform findPlayer(string name, bool layer) {
        GameObject[] p = GameObject.FindGameObjectsWithTag("Player");
        Transform t = null;
        foreach (GameObject o in p) if (o.GetComponent<Respawn>().playerName == name) t = o.transform;
        if (!t) Debug.Log("No player by that name.");
        if (layer) t = t.transform.Find("FirstPersonCharacter").transform;
        return t;
    }
    
    public GameObject FindLocalPlayer() {
        foreach(GameObject cur in GameObject.FindGameObjectsWithTag("Player")) {
            if(cur.GetComponent<Respawn>().isLocalPlayer) {
                return cur;
            }
        }
        Debug.Log ("Couldn't find local player");
        return null;
    }

	public bool CheckPOV () {
        return isLocalPlayer;
	}
    
    public NetworkConnection getConnection() {
        return connectionToClient;
    }
    
    public void SetPickupableAuthority (string newer, NetworkInstanceId id) {
        CmdSetPickupableAuthority(newer, id);
    }
    
    [Command]
    void CmdSetPickupableAuthority(string newer, NetworkInstanceId id) {
        NetworkServer.FindLocalObject(id).GetComponent<Pickupable>().NewAuthority(newer);
    }
    
    public void RemoteGrab (string player, NetworkInstanceId id) {
        CmdRemoteGrab(player, id);
    }
    
    [Command]
    public void CmdRemoteGrab (string player, NetworkInstanceId id) {
        NetworkServer.FindLocalObject(id).GetComponent<Pickupable>().GrabRelay(player);
    }
    
    public void resetSpawn (NetworkInstanceId id) {
        CmdResetSpawn(id);
    }
    
    [Command]
    void CmdResetSpawn (NetworkInstanceId id) {
        RpcResetSpawn(NetworkServer.FindLocalObject(id)); //passing gameobject to RPC looks up object for you
    }
    
    [ClientRpc]
    void RpcResetSpawn (GameObject obj) {
        GameObject p = FindLocalPlayer();
        if (p.GetComponent<Respawn>().spawnpoint == obj) {
			p.GetComponent<Respawn>().spawnpoint = GameObject.Find("spawn");
			GameObject.Find("Text").GetComponent<Statusmsg>().Show("spawn removed");
		}
    }
}
