using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Pickupable : NetworkBehaviour {

    GameObject localPlayer;
    string localName; //unique for ever instance on every client
    string ownerName; //player that currently owns this object

	Rigidbody rigB;
	bool holding = false;
	bool drop = false;
    string playerName;
	Material natmat;
	public Material zero_grav_mat;
	public Material fixed_pos_mat;
    
    [SyncVar]
	public int physics; //0 = grav, 1 = anti-grav, 2 = fixed
	
	void Awake () {
		rigB = GetComponent<Rigidbody>();
		natmat = GetComponent<Renderer>().material;
		if (physics == 1) GetComponent<Renderer>().material = zero_grav_mat;
		if (physics == 2) GetComponent<Renderer>().material = fixed_pos_mat;
		updatePhysics(physics);
	}
    
    void Start () {
        localPlayer = GameObject.FindWithTag("Player").GetComponent<Respawn>().FindLocalPlayer();
        localName = localPlayer.GetComponent<Respawn>().playerName;
    }
    
    public void PostStart (string p) {
        ownerName = p;
        RpcPostStart(p);
    }
    
    [ClientRpc]
    void RpcPostStart (string p) {
        playerName = GameObject.FindWithTag("Player").GetComponent<Respawn>().findPlayer(p, false).GetComponent<Respawn>().playerName;
        if (playerName == GameObject.FindWithTag("Player").GetComponent<Respawn>().FindLocalPlayer().GetComponent<Respawn>().playerName) {
            CmdGrab(playerName);
            holding = true;
            drop = false;
		}
    }
    
	void updatePhysics(int p) {
		if (p == 0) {
			GetComponent<Rigidbody>().useGravity = true;
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		}
		if (p == 1) {
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		}
		if (p == 2) {
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		}
	}
	
	void OnMouseOver() {
		Vector3 distance = Camera.main.transform.position - transform.position;
		if (distance.sqrMagnitude<=9 && !transform.parent) {
			GUICrossHair.grabmode = 1;
			if (Input.GetMouseButtonDown(0) && physics != 2) { //on left mouse click
                GrabSubvertAuthority(localName);
                NewOwner(localName);
                holding = true;
                drop = false;
				if (tag == "Respawn")
					resetSpawn();
			} else if (physics == 2) GUICrossHair.grabmode = 2;
		} else GUICrossHair.grabmode = 0;
	}
	
	void resetSpawn() {
		if (GameObject.Find("FPSController").GetComponent<Respawn>().spawnpoint != GameObject.Find("spawn")) {
			GameObject.Find("FPSController").GetComponent<Respawn>().spawnpoint = GameObject.Find("spawn");
			GameObject.Find("Text").GetComponent<Statusmsg>().Show("spawn removed");
		}
	}
    
    public void GrabSubvertAuthority(string player) {
        localPlayer.GetComponent<Respawn>().RemoteGrab(player, this.gameObject.GetComponent<NetworkIdentity>().netId);
    }
    
    public void GrabRelay(string player) { //server only
        Grab(player);
        RpcGrab(player);
    }
    
    [Command]
	public void CmdGrab(string player) {
        Grab(player);
        RpcGrab(player);
	}
    
    [ClientRpc]
    void RpcGrab(string player) {
        Grab(player);
        Color color = GetComponent<Renderer>().material.color;
		color.a = 0.5f;
		GetComponent<Renderer>().material.color = color;
    }
    
    
    void Grab(string player) {
        GetComponent<Rigidbody>().useGravity = false;
		GetComponent<Collider>().enabled = false;
        GameObject g1 = GameObject.FindWithTag("Player");
		Transform t = g1.GetComponent<Respawn>().findPlayer(player, true);
        if (hasAuthority) this.transform.position = t.position + t.forward*2f;
		this.transform.parent = t;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }
    
    [Command]
	public void CmdActionButton(int ph) {
        physics = ph;
        RpcActionButton(ph);
    }
    
    [ClientRpc]
    void RpcActionButton(int ph) {
        Material current = natmat;
        if (ph == 1) current = zero_grav_mat;
        if (ph == 2) current = fixed_pos_mat;
        Color color = current.color;
        color.a = 0.5f;
        current.color = color;
        GetComponent<Renderer>().material = current;
    }
    
    [Command]
    public void CmdDrop() {
        Drop();
        RpcDrop();
    }
    
    [ClientRpc]
    void RpcDrop() {
        Drop();
		Color color = GetComponent<Renderer>().material.color;
		color.a = 1.0f;
		GetComponent<Renderer>().material.color = color;
    }
    
    void Drop() {
        GetComponent<Collider>().enabled = true;
		this.transform.parent = null;
		updatePhysics(physics);
    }

	void Update () {
        if (transform.parent && GetComponent<Rigidbody>().useGravity) GetComponent<Rigidbody>().useGravity = false;
        if (physics == 2 && !holding) return;
		if (Input.GetKeyDown("escape") || Input.GetMouseButtonUp(0)) drop = true;
		if (rigB.position.y < -50) {
			if (tag == "Respawn") resetSpawn();
			Destroy (gameObject);
		}
        if (!transform.parent) return;
		if (holding) {
			if (!drop) {
				if (Input.GetMouseButtonDown(1)) { //on right mouse click
                    CmdActionButton((physics+1)%3);
				}
				
				if (Input.GetMouseButtonDown(2)) //on middle mouse click
					Debug.Log("Pressed middle click.");
			}
			
			if (drop && localPlayer.GetComponent<CharacterController>().isGrounded) {
                Drop();
                Color color = GetComponent<Renderer>().material.color;
                color.a = 1.0f;
                GetComponent<Renderer>().material.color = color;
                CmdDrop();
				holding = false;
				if (tag == "Respawn" && physics == 0) {
					GameObject.Find("FPSController").GetComponent<Respawn>().spawnpoint = gameObject;
					GameObject.Find("Text").GetComponent<Statusmsg>().Show("spawn set");
				}
				drop = false;
			}
		}
	}
    
    void NewOwner (string newer) { //relay from client to server
        localPlayer.GetComponent<Respawn>().SetPickupableAuthority(newer, this.gameObject.GetComponent<NetworkIdentity>().netId);
    }
    
    public void NewAuthority (string newer) { //called on server only
        GameObject g1 = GameObject.FindWithTag("Player");
        Transform t1 = g1.GetComponent<Respawn>().findPlayer(ownerName, false);
        Transform t2 = g1.GetComponent<Respawn>().findPlayer(newer, false);
        NetworkConnection c1 = t1.gameObject.GetComponent<Respawn>().getConnection();
        NetworkConnection c2 = t2.gameObject.GetComponent<Respawn>().getConnection();
        this.gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(c1);
        this.gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(c2);
        ownerName = newer;
    }
}
