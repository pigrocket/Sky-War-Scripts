using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Inventory : MonoBehaviour {

	GameObject displayname;
	GameObject displayyen;
	GameObject displaydollars;

	GameObject moneysystem;
	GameObject inv;
	GameObject interf;
	Camera cam;
	static int r = 4;
	static int c = 3;

	//shop Prefabs have been moved to FPSController/respawn
	
	public int[] costY;
	public int[] costD;
	public string[] shopNames;
	List<Transform> cells;
	List<Transform> rows;
	List<Transform> cols;
	
	float[] col_left = new float[c];
	float[] col_right = new float[c];
	float[] row_top = new float[r];
	float[] row_bottom = new float[r];
	
	bool pre_open = false;
	bool open = false;
	//bool lockmouse = true;
	
	// Use this for initialization
	void Start () {
		
		cam = transform.parent.GetComponent<Camera>();
		
		displayname = GameObject.Find("ItemName");
		displayyen = GameObject.Find("ItemYen");
		displaydollars = GameObject.Find("ItemDollars");
		moneysystem = GameObject.Find("Money");
		
		inv = transform.Find("Inventory").gameObject;
		interf = transform.Find("Inventory/positioners").gameObject;
		
		cells = getChildren(interf.transform);
		rows = getChildren(cells[0]);
		cols = getChildren(cells[1]);
		
		List<Transform> box;
		
		int i = 0;
		while (i < rows.Count) {
			box = getChildren(rows[i]);
			row_top[i] = cam.WorldToScreenPoint(box[0].position).y;
			row_bottom[i] = cam.WorldToScreenPoint(box[1].position).y;
			i++;
		}
		
		i = 0;
		while (i < cols.Count) {
			box = getChildren(cols[i]);
			col_left[i] = cam.WorldToScreenPoint(box[0].position).x;
			col_right[i] = cam.WorldToScreenPoint(box[1].position).x;
			i++;
		}
			
		inv.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		displayname.GetComponent<Text>().text = "";
		displayyen.GetComponent<Text>().text = "";
		displaydollars.GetComponent<Text>().text = "";
		if (Input.GetKeyDown("escape")) pre_open = !pre_open;
	
		if (open != pre_open) {
			open = !open;
			inv.SetActive(open);
            transform.parent.parent.GetComponent<FirstPersonController>().enabled = !open;
            //get parent player -> ().relay -> ().Cmd color "player" children renderers on all clients except local 
		}
		
		if (open) {
			Vector3 pos = Input.mousePosition;
			//rows
			int i = 0;
			while (i < row_top.Length) {
				if (row_top[i] > pos.y && row_bottom[i] < pos.y) break;
				i++;
			}
			//cols
			int j = 0;
			while (j < col_left.Length) {
				if (col_left[j] < pos.x && col_right[j] > pos.x) break;
				j++;
			}
			
			if (i < row_top.Length && j < col_left.Length) {
				int index = ((i)*c + (j+1) - 1);
				//rotate object
				int d = costD[index]%100;
				string dol = "." + d;
				if (d < 10) dol = ".0" + d;
				displayname.GetComponent<Text>().text = shopNames[index];
				displayyen.GetComponent<Text>().text = "¥ " + costY[index];
				displaydollars.GetComponent<Text>().text = "$ " + costD[index]/100 + dol ;;
				if (Input.GetMouseButtonDown(0)) {
					pre_open = false;
					if (moneysystem.GetComponent<MoneySystem>().GetDollars() >= costD[index] && moneysystem.GetComponent<MoneySystem>().GetYen() >= costY[index]) {
						moneysystem.GetComponent<MoneySystem>().AddMoney(-1*costY[index],-1*costD[index]);
						transform.parent.parent.GetComponent<Respawn>().Buy(index);
					} else GameObject.Find("Text").GetComponent<Statusmsg>().Show("not enough funds");
				}
			}
			
			if (Input.GetMouseButtonDown(0)) pre_open = false;
				
			//Debug.Log("box: " + i + " " + j);
		}
	}
	
	void OnGUI() {
		if (open) Cursor.lockState = CursorLockMode.None;
		else Cursor.lockState = CursorLockMode.Locked;
	}
	
	List<Transform> getChildren(Transform transform) {
		List<Transform> list = new List<Transform>();
		foreach (Transform child in transform) list.Add(child);
		return list;
	}
}