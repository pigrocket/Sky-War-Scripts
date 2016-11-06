using UnityEngine;
using System.Collections;
using UnityEngine.UI;
 
public class MoneySystem : MonoBehaviour {

    public int yen;
	public int dollars; //current balance
    
    void Start()
    {

    }
 
//Checks if you have enough money to buy item with cost, if you do buy it and return true. Otherwise, return false.
    public bool BuyItem(int ycost, int dcost)
    {
        if (yen - ycost >= 0 && dollars - dcost >= 0)
        {
            yen -= ycost;
			dollars -= dcost;
            return true;
        }
        else
        {
            return false;
        }
    }
 
//Simply return the balance
    public int GetYen()
    {
        return yen;
    }
	
	public int GetDollars()
	{
		return dollars;
	}
 
//Add some money to the balance.
    public void AddMoney(int yens, int dollar)
    {
        yen += yens;
		dollars += dollar;
    }
	
	void OnGUI () {
		int d = dollars%100;
		string dol = "." + d;
		if (d < 10) dol = ".0" + d;
		gameObject.GetComponent<Text>().text = "¥ " + yen +"\n$ " + dollars/100 + dol;
	}
}
 