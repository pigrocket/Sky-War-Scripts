using UnityEngine;
using System.Collections;

public class icosphere : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnMouseOver() {
		Vector3 distance = Camera.main.transform.position - transform.position;
		if (distance.sqrMagnitude<=9) {
			GUICrossHair.grabmode = 1;
			if (Input.GetMouseButtonDown(0)) {
				transform.parent.GetComponent<Animator>().Play("altartap");
				GameObject.Find("Money").GetComponent<MoneySystem>().AddMoney(20,103);
			}	
		} else GUICrossHair.grabmode = 0;
	}
}
