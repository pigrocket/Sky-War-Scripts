using UnityEngine;
using System.Collections;

public class hurtflash : MonoBehaviour {

    void OnEnable() {
        Color newColor = new Color(1, 1, 1, 1);
        transform.GetComponent<Renderer>().material.color = newColor;
		StartCoroutine(FadeTo(0.0f, 1.1f));
    }
    
    IEnumerator FadeTo(float aValue, float aTime)
    {
    float alpha = transform.GetComponent<Renderer>().material.color.a;
    for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
    {
        Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha,aValue,t));
        transform.GetComponent<Renderer>().material.color = newColor;
        yield return null;
    }
 }
}