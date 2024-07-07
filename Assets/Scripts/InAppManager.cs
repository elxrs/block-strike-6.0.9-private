using UnityEngine;

public class InAppManager : MonoBehaviour
{
	private void Start()
	{
		Init();
	}
    
	public static void Init()
	{
	}
    
	public static string GetPrice(string sku)
	{
		if (Application.isEditor)
		{
			return "0,00$";
		}
		return "0,00$";
	}
    
	public static void Purchase(string sku)
	{
	}
    
	public static void Consume(string sku)
	{
	}
}
