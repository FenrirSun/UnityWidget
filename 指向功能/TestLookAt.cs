using System;
using UnityEngine;

///<summary>
///朝向功能测试脚本
///</summary>
public class TestLookAt : MonoBehaviour
{
    public enum Axis
    {
        Forward = 1,
        Back,
        Up,
        Down,
        Left,
        Right
    }

    //关节
    public Joints[] joints;
    //融合时间
    public float blendTime = 0.2f;
    //目标
    GameObject m_Target;
    //开始指向时间
    float startTime;

    //创建指向
    void CreateLookat()
    {
        if (joints == null || joints.Length == 0)
            return;

        //创建一个目标
        var targetPosition = joints[0].bone.position + gameObject.transform.rotation * Vector3.forward;
        m_Target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_Target.transform.position = targetPosition;
        m_Target.transform.rotation = Quaternion.identity;
        m_Target.transform.localScale = Vector3.one * 0.1f;

        startTime = Time.time;
    }

    private void LateUpdate()
    {
        float totalWeight = Mathf.Clamp01((Time.time - startTime) / blendTime);
        if (m_Target != null && joints != null)
        {
            for (int i = 0; i < joints.Length; ++i)
            {
                SingleBoneLookAt(joints[i], m_Target.transform, totalWeight);
            }
        }
    }

    private void SingleBoneLookAt(Joints joint, Transform target, float totalWeight)
    {
        var selfTran = joint.bone;

        //////////////////////////////////////////////////////////////////////////////////////////////
        //方法一
        //计算出自身到目标位置的方向向量，此方法只能让forward朝向目标，可以设置朝向速度
        //selfTran.rotation = Quaternion.Slerp(selfTran.rotation, Quaternion.LookRotation(target.position - selfTran.position), 1 * Time.deltaTime);

        //////////////////////////////////////////////////////////////////////////////////////////////
        //方法二
        //这种算法可以让某一跟指定的轴朝向目标，也可以控制其他轴的对应位置
        //但是不能让其他轴的对应位置和之前一致，因此不适用于动画系统中改变骨骼

        //var pos = selfTran.position;

        //var rotation = selfTran.rotation;

        //var targetDir = target.transform.position - pos;

        //var fromDir = selfTran.up; //rotation * GetAxisVector(joints[0].axis);

        //var axis = Vector3.Cross(fromDir, targetDir).normalized;
        //var angle = Vector3.Angle(fromDir, targetDir);

        //Quaternion upToForward = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        //Quaternion rightToForward = Quaternion.FromToRotation(Vector3.right, Vector3.forward);

        //var forwardRot = Quaternion.LookRotation(targetDir.normalized, selfTran.rotation * -Vector3.right);

        //selfTran.rotation = forwardRot * rightToForward;

        //////////////////////////////////////////////////////////////////////////////////////////////
        //方法三
        //这种方法可以以当前指定轴最短直线路径的方法朝向目标，可用于动画系统中的指向

        var pos = selfTran.position;
        var rotation = selfTran.rotation;
        var targetDir = target.transform.position - pos;

        //指定哪根轴朝向目标
        var fromDir = selfTran.rotation * GetAxisVector(joint.axis);
        //计算垂直于当前方向和目标方向的轴
        var axis = Vector3.Cross(fromDir, targetDir).normalized;
        //计算当前方向和目标方向的夹角
        var angle = Vector3.Angle(fromDir, targetDir) * joint.weight * 0.01f * totalWeight;
        angle = Mathf.Clamp(angle, -joint.Angle, joint.Angle);
        //将当前朝向向目标方向旋转一定角度，这个角度值可以做插值
        selfTran.rotation = Quaternion.AngleAxis(angle, axis) * rotation;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("LookAt"))
        {
            CreateLookat();
        }

        if (GUILayout.Button("EndLook"))
        {
            if (m_Target)
            {
                GameObject.Destroy(m_Target);
                m_Target = null;
            }
        }
    }

    Vector3 GetAxisVector(Axis axis)
    {
        switch (axis)
        {
            case Axis.Forward:
                return Vector3.forward;
            case Axis.Back:
                return Vector3.back;
            case Axis.Up:
                return Vector3.up;
            case Axis.Down:
                return Vector3.down;
            case Axis.Left:
                return Vector3.left;
            case Axis.Right:
                return Vector3.right;
        }

        return Vector3.forward;
    }

    [Serializable]
    public class Joints
    {
        public Transform bone;
        public Axis axis = Axis.Up;
        public float Angle = 45.0f;
        public int weight = 100;
    }
}
