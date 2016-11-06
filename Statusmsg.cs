using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Statusmsg : MonoBehaviour {

	public float FadeoutTime;
	 
	void Awake() {
		gameObject.GetComponent<Text>().CrossFadeAlpha(0.0f, 0f, false);
	}
	
	public void Show(string s) {
		GetComponent<Text>().text = s;
		Debug.Log(s);
		gameObject.GetComponent<Text>().CrossFadeAlpha(1.0f, 0f, false);
		gameObject.GetComponent<Text>().CrossFadeAlpha(0.0f, 5, false);
	}
}