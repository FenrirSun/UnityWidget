using UnityEngine;
using System.Collections;

public class TestStart : MonoBehaviour 
{

	void Start () 
    {
        UIManager.Instance.Open<TestWnd>(UIManager.EUILayer.WndLayer);
	}
	

}
