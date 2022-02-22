using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 合并模型，用于换装部件的动态组合
/// 这里会把使用同一骨骼的模型合并成一个，但是材质球分别使用部件各自的
/// 骨骼用主物体的骨骼，可应用原来的动画
/// </summary>
public class CharacterCombineSimple : MonoBehaviour
{
    // 目标物体（必须是骨骼的父物体，不然蒙皮失效）
    public GameObject target;
    //根骨骼
    public Transform BoneRoot;

    // 物体所有的部分
    private List<GameObject> targetParts = new List<GameObject>();
    private string[] defaultEquipPartPaths = new string[9];

    void Start()
    {
        // 把FBX的模型按部件分别放入Resources下对应的文件夹里，可以留空，模型需要蒙皮，而且所有模型使用同一骨骼
        // 最后的M是Fbx的模型，需要的Unity3D里设置好材质和贴图，部件贴图要勾选Read/Write Enabled
        defaultEquipPartPaths[0] = "CombineMesh/clothes";
        defaultEquipPartPaths[1] = "CombineMesh/hari";
        defaultEquipPartPaths[2] = "CombineMesh/eyes";
        defaultEquipPartPaths[3] = "CombineMesh/face";
        //defaultEquipPartPaths[4] = "CombineMesh/body";

        //Destroy(target.GetComponent<SkinnedMeshRenderer>());

        for (int i = 0; i < defaultEquipPartPaths.Length; i++) {
            UnityEngine.Object o = Resources.Load(defaultEquipPartPaths[i]);
            if (o) {
                GameObject go = Instantiate(o) as GameObject;
                go.transform.parent = target.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = new Quaternion();
                targetParts.Add(go);
            }
        }

        StartCoroutine(DoCombine());
    }

    /// <summary>
    /// 使用延时，不然某些GameObject还没有创建
    /// </summary>
    /// <returns></returns>
    IEnumerator DoCombine()
    {
        yield return null;
        Combine(target.transform);
    }

    /// <summary>
    /// 合并蒙皮网格，刷新骨骼
    /// 注意：合并后的网格会使用同一个Material
    /// </summary>
    /// <param name="root">角色根物体</param>
    private void Combine(Transform root)
    {
        float startTime = Time.realtimeSinceStartup;
        Transform[] transforms = BoneRoot.GetComponentsInChildren<Transform>();
        List<Transform> boneList = new List<Transform>();
        for(int i = 0; i < targetParts.Count; ++i) {
            boneList.Clear();
            //获取骨骼信息
            SkinnedMeshRenderer smr = targetParts[i].GetComponentInChildren<SkinnedMeshRenderer>();
            foreach (Transform bone in smr.bones) {
                foreach (Transform item in transforms) {
                    if (item.name != bone.name) continue;
                    boneList.Add(item);
                    break;
                }
            }

            // 修改节点，设置根骨骼
            smr.transform.SetParent(target.transform);
            foreach (Transform bone in boneList) {
                if (bone.name == smr.rootBone.name) {
                    smr.rootBone = bone;
                    break;
                }
            }
            smr.bones = boneList.ToArray();
        }

        // 销毁所有部件的骨骼部分
        foreach (GameObject goTemp in targetParts) {
            if (goTemp) {
                Destroy(goTemp);
            }
        }
        //把骨骼放到最后
        BoneRoot.SetAsLastSibling();

        Debug.Log("合并耗时 : " + (Time.realtimeSinceStartup - startTime) * 1000 + " ms");
    }

}