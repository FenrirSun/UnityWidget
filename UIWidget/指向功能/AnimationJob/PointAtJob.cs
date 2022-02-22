using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Animations;

namespace Entity
{
    /// <summary>
    /// 指向Job
    /// @data: 2019-06-04
    /// @author: sunhong
    /// </summary>
    public struct PointAtJob : IAnimationJob
    {
        // 关节及其轴向信息
        public NativeArray<PointAtJointInfo> joints;
        // 自身的根目标
        public TransformSceneHandle selfRoot;
        // 根节点缩放
        public Vector3 rootScale;

        public void ProcessRootMotion(AnimationStream stream)
        {
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            Solve(stream);
        }

        private void Solve(AnimationStream stream)
        {
            if (!joints.IsCreated || joints.Length == 0)
                return;

            for (int i = 0; i < joints.Length; ++i)
            {
                SolveSingle(stream, joints[i]);
            }
        }

        private void SolveSingle(AnimationStream stream, PointAtJointInfo jointInfo)
        {
            //根节点的位置和旋转
            var rootPosition = selfRoot.GetPosition(stream);
            var rootRotation = selfRoot.GetRotation(stream);
            var rootScale = selfRoot.GetLocalScale(stream);

            //这里GetPosition和GetRotation方法并不是真的世界坐标
            //位置直接加减即可，旋转需要先转到世界坐标再转回来
            var jointPosition = jointInfo.joint.GetPosition(stream);
            jointPosition.x *= rootScale.x;
            jointPosition.y *= rootScale.y;
            jointPosition.z *= rootScale.z;
            jointPosition += rootPosition;

            var jointRotation = jointInfo.joint.GetRotation(stream);
            var jointWorldRotation = rootRotation * jointRotation;

            //根节点世界旋转的逆向
            var inversRootRotation = Quaternion.Inverse(rootRotation);
            //目标位置的世界坐标是正确的，不需要转换
            var targetPosition = jointInfo.target.GetPosition(stream);
            //这里的原始朝向需要加上根节点的偏移
            Vector3 fromDir = rootRotation * (jointRotation * jointInfo.axis);
            Vector3 toDir = targetPosition - jointPosition;

            //计算旋转的轴向和角度,都是世界坐标下的
            var axis = Vector3.Cross(fromDir, toDir).normalized;
            var angle = Vector3.Angle(fromDir, toDir);
            angle *= jointInfo.weight * 0.01f;
            angle = Mathf.Clamp(angle, jointInfo.maxAngle * -1, jointInfo.maxAngle);
            var jointToTargetRotation = Quaternion.AngleAxis(angle, axis);

            //通过世界旋转计算轴向偏移，再逆向算回来
            jointWorldRotation = jointToTargetRotation * jointWorldRotation;
            jointRotation = inversRootRotation * jointWorldRotation;
            jointInfo.joint.SetRotation(stream, jointRotation);
        }
    }

    public struct PointAtJointInfo
    {
        //骨骼关节
        public TransformStreamHandle joint;
        //旋转轴
        public Vector3 axis;
        //最大限制角度
        public float maxAngle;
        //朝向目标
        public TransformSceneHandle target;
        //权重
        public int weight;
    }
}