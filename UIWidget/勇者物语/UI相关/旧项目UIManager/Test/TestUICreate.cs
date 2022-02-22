using UnityEngine;
using System.Collections;

public class TestUICreate : MonoBehaviour {

    IEnumerator Start()
    {
        yield return new WaitForSeconds(2);
        TestClass1 cla = UIWndManager.instance.Open<TestClass1>(UIWndManager.eUILayer.MainLayer);
        //UIWndManager.instance.Open<TestClass1>(UIWndManager.eUILayer.MainLayer);
        yield return new WaitForSeconds(1);

        UIWndManager.instance.Close(typeof(TestClass1));
        //UIWndManager.instance.Open<TestClass1>(UIWndManager.eUILayer.MainLayer);
    }
}
