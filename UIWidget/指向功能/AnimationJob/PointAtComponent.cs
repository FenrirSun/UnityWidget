using Edu100.Table;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace Entity
{
    /// <summary>
    /// 指向组件
    /// 可以让角色的头，手等固定骨骼指向目标
    /// @data: 2019-06-04
    /// @author: sunhong
    /// </summary>
    public class PointAtComponent : BaseComponent
    {
        //头部关节
        private Joints headJoint;
        //指向关节，不大于5个
        private List<Joints> joints;
        //从animator取来的graph
        private PlayableGraph m_Graph;
        //默认输出，Animator自带
        private PlayableOutput oriOutput;
        //朝向输出
        private AnimationPlayableOutput pointatOutput;
        //结束回调
        private Action<BaseEntity> callBack;
        //创建的指向脚本playablle
        AnimationScriptPlayable m_pointAtPlayable;
        //缓存获取的NativeArray，用于结束时清理
        Unity.Collections.NativeArray<PointAtJointInfo> nativeArray;

        private float minAngle = -50.0f;
        private float maxAngle = 50.0f;
        private float blendTime = 0.5f;
        private float pointlookAtWeight = 1;

        /// <summary>
        /// 开始指向/朝向 单目标
        /// </summary>
        /// <param name="_type">指向类型</param>
        /// <param name="_target">指向目标</param>
        /// <param name="_blendTime">融合时间</param>
        public void StartPointAt(int _pointAtId, Transform _target, TargetPoint _targetType = TargetPoint.root, float _blendTime = 0.5f)
        {
            if (_target == null || entity == null || entity.GameObject == null)
            {
                this.LogError($"无法指向目标，目标为空或自身为空");
                return;
            }

            //找动画状态机
            Animator animator = entity.GameObject.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                this.LogError($"无法指向目标，找不到Animator组件");
                return;
            }

            //如果之前的看向还没清理，先清理
            if (endLookTime != 0 || startLookTime != 0)
            {
                OnClear();
                endLookTime = 0;
            }

            //找关节
            FindAllJoints(_pointAtId);
            if (joints == null || joints.Count == 0)
            {
                this.LogError($"无法指向目标，找不到对应关节：{entity.Name}");
                return;
            }

            blendTime = Mathf.Max(0.01f, _blendTime);
            m_Graph = animator.playableGraph;
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            oriOutput = m_Graph.GetOutput(0);
            Playable oriPlayble = oriOutput.GetSourcePlayable();

            //看向目标
            pointatOutput = AnimationPlayableOutput.Create(m_Graph, "PointAt", animator);
            //animator.fireEvents = false;
            var lookAtJob = new PointAtJob()
            {
                selfRoot = animator.BindSceneTransform(entity.Transform),
                rootScale = entity.Transform.localScale
            };

            nativeArray = new Unity.Collections.NativeArray<PointAtJointInfo>(joints.Count, Unity.Collections.Allocator.TempJob);
            lookAtJob.joints = nativeArray;

            for (int i = 0; i < joints.Count; ++i)
            {
                Joints joint = joints[i];
                var temp = new PointAtJointInfo();
                temp.joint = animator.BindStreamTransform(joint.joint);
                temp.axis = GetAxisVector(joint.axis);
                temp.maxAngle = joint.msxAngle;
                temp.weight = joint.weight;
                temp.target = animator.BindSceneTransform(FindTargetPoint(_target, _targetType));
                lookAtJob.joints[i] = temp;
            }

            m_pointAtPlayable = AnimationScriptPlayable.Create(m_Graph, lookAtJob);
            //以原animator的playerable作为此output的输入
            m_pointAtPlayable.AddInput(oriPlayble, 0, 1.0f);
            pointatOutput.SetSourcePlayable(m_pointAtPlayable);

            //设置权重，开始播放
            pointlookAtWeight = 0;
            SetWeight(pointlookAtWeight);
            m_Graph.Stop();
            m_Graph.Play();

            startLookTime = Time.time;
        }

        /// <summary>
        /// 开始变看向一个目标边指向另一个目标
        /// </summary>
        /// <param name="_type">指向类型</param>
        /// /// <param name="_headTarget">看向的目标</param>
        /// <param name="_target">指向目标</param>
        /// <param name="_blendTime">融合时间</param>
        public void StartLookAndPointAt(int _pointAtId, Transform _headTarget, Transform _target, TargetPoint _targetType = TargetPoint.root, float _blendTime = 0.5f)
        {
            if (_target == null || entity == null || entity.GameObject == null)
            {
                this.LogError($"无法指向目标，目标为空或自身为空");
                return;
            }

            Table_Role_Anim_Point_At tab = Table_Role_Anim_Point_At.GetPrimary(_pointAtId);
            if (tab == null)
            {
                this.LogError($"找不到角色动画指向表数据，Table_Role_Anim_Point_At id:{_pointAtId}!");
                return;
            }
            else if (!tab.issupportpointtwo)
            {
                this.LogError($"该动画的指向类型不支持指向两个目标，Table_Role_Anim_Point_At id:{_pointAtId}!");
                StartPointAt(_pointAtId, _target, _targetType, _blendTime);
                return;
            }

            //找动画状态机
            Animator animator = entity.GameObject.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                this.LogError($"无法指向目标，找不到Animator组件");
                return;
            }

            //如果之前的看向还没清理，先清理
            if (endLookTime != 0 || startLookTime != 0)
            {
                OnClear();
                endLookTime = 0;
            }

            //找关节
            FindAllJoints(_pointAtId, true);
            if (headJoint == null || joints == null || joints.Count == 0)
            {
                this.LogError($"无法指向目标，找不到对应关节：{entity.Name}");
                return;
            }

            blendTime = Mathf.Max(0.01f, _blendTime);
            m_Graph = animator.playableGraph;
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            PlayableOutput oriOutput = m_Graph.GetOutput(0);
            Playable oriPlayble = oriOutput.GetSourcePlayable();

            //看向目标
            pointatOutput = AnimationPlayableOutput.Create(m_Graph, "PointAt", animator);

            var lookAtJob = new PointAtJob()
            {
                selfRoot = animator.BindSceneTransform(entity.Transform),
            };

            lookAtJob.joints = new Unity.Collections.NativeArray<PointAtJointInfo>(joints.Count + 1, Unity.Collections.Allocator.Temp);
            //头部朝向信息
            var headJointInfo = new PointAtJointInfo();
            headJointInfo.joint = animator.BindStreamTransform(headJoint.joint);
            headJointInfo.axis = GetAxisVector(headJoint.axis);
            headJointInfo.maxAngle = headJoint.msxAngle;
            headJointInfo.weight = headJoint.weight;
            headJointInfo.target = animator.BindSceneTransform(FindTargetPoint(_headTarget, TargetPoint.root));
            lookAtJob.joints[0] = headJointInfo;

            //其他朝向信息
            for (int i = 0; i < joints.Count; ++i)
            {
                var temp = new PointAtJointInfo();
                temp.joint = animator.BindStreamTransform(joints[i].joint);
                temp.axis = GetAxisVector(joints[i].axis);
                temp.maxAngle = joints[i].msxAngle;
                temp.weight = joints[i].weight;
                temp.target = animator.BindSceneTransform(FindTargetPoint(_target, _targetType));
                lookAtJob.joints[i + 1] = temp;
            }

            m_pointAtPlayable = AnimationScriptPlayable.Create(m_Graph, lookAtJob);
            //以原animator的playerable作为此output的输入
            m_pointAtPlayable.AddInput(oriPlayble, 0, 1.0f);
            pointatOutput.SetSourcePlayable(m_pointAtPlayable);

            //设置权重，开始播放
            pointlookAtWeight = 0;
            SetWeight(pointlookAtWeight);
            m_Graph.Stop();
            m_Graph.Play();

            startLookTime = Time.time;
        }

        /// <summary>
        /// 结束指向
        /// </summary>
        /// <param name="_blendTime">动画混合时间</param>
        /// <param name="_cb">回调</param>
        public void EndPointAt(float _blendTime = 0.5f, Action<BaseEntity> _cb = null)
        {
            callBack = _cb;
            blendTime = Mathf.Max(0.01f, _blendTime);
            pointlookAtWeight = 1;
            startLookTime = 0;
            endLookTime = Time.time;
        }

        /// <summary>
        /// 快速结束指向
        /// </summary>
        /// <param name="_cb"></param>
        public void QuickEndPointAt(Action<BaseEntity> _cb = null)
        {
            callBack = _cb;
            callBack?.Invoke(entity);
            OnClear();
        }

        /// <summary>
        /// 查找关节
        /// </summary>
        private void FindAllJoints(int _pointTabId, bool _findHead = false)
        {
            Table_Role_Anim_Point_At tab = Table_Role_Anim_Point_At.GetPrimary(_pointTabId);
            if (tab == null)
            {
                this.LogError($"找不到角色动画指向表数据， id:{_pointTabId}!");
                return;
            }

            joints = new List<Joints>();
            var head = FindSingleJoint(tab.bone_head, tab.axis_head, tab.max_angle_head, tab.weight_head);
            if (head != null)
            {
                if (_findHead)
                    headJoint = head;
                else
                    joints.Add(head);
            }
            var joint2 = FindSingleJoint(tab.bone_2, tab.axis_2, tab.max_angle_2, tab.weight_2);
            if (joint2 != null)
                joints.Add(joint2);
            var joint3 = FindSingleJoint(tab.bone_3, tab.axis_3, tab.max_angle_3, tab.weight_3);
            if (joint3 != null)
                joints.Add(joint3);
            var joint4 = FindSingleJoint(tab.bone_4, tab.axis_4, tab.max_angle_4, tab.weight_4);
            if (joint4 != null)
                joints.Add(joint4);
            var joint5 = FindSingleJoint(tab.bone_5, tab.axis_5, tab.max_angle_5, tab.weight_5);
            if (joint5 != null)
                joints.Add(joint5);
        }

        private Joints FindSingleJoint(string _bone, int _axis, float _angle, int _weight)
        {
            if (string.IsNullOrEmpty(_bone) || _axis == 0)
                return null;

            var bone = entity.Transform.GetChildByName(_bone);
            if (bone == null)
                return null;

            var temp = new Joints()
            {
                joint = bone,
                axis = (Axis)_axis,
                msxAngle = _angle == 0 ? 45f : _angle,
                weight = Mathf.Clamp(_weight, 0, 100)
            };

            return temp;
        }

        /// <summary>
        /// 查找目标的位置
        /// </summary>
        private Transform FindTargetPoint(Transform _target, TargetPoint _type)
        {
            Transform result = null;
            switch (_type)
            {
                case TargetPoint.root:
                    result = _target;
                    break;
                case TargetPoint.head:
                    result = _target.GetChildByName("Bip001 Head");
                    break;
                case TargetPoint.rHand:
                    result = _target.GetChildByName("r_hand");
                    break;
            }
            if (result == null)
                result = _target;
            return result;
        }

        /// <summary>
        /// 设置权重
        /// </summary>
        /// <param name="_weight"></param>
        void SetWeight(float _weight)
        {
            if (!pointatOutput.Equals(AnimationPlayableOutput.Null))
            {
                pointatOutput.SetWeight(_weight);
            }
        }

        public PointAtComponent(BaseEntity _entity) : base(_entity)
        { }

        float startLookTime;
        float endLookTime;
        internal override void FixedUpdate()
        {
            if (startLookTime != 0)
            {
                if (Time.time - startLookTime <= blendTime)
                {
                    pointlookAtWeight = Mathf.Lerp(0, 1, (Time.time - startLookTime) / blendTime);
                    SetWeight(pointlookAtWeight);
                }
                else
                {
                    pointlookAtWeight = 1;
                    SetWeight(pointlookAtWeight);
                    startLookTime = 0;
                }
            }
            else if (endLookTime != 0)
            {
                if (Time.time - endLookTime <= blendTime)
                {
                    pointlookAtWeight = Mathf.Lerp(1, 0, (Time.time - endLookTime) / blendTime);
                    SetWeight(pointlookAtWeight);
                }
                else
                {
                    pointlookAtWeight = 0;
                    SetWeight(pointlookAtWeight);
                    callBack?.Invoke(entity);
                    OnClear();
                }
            }
        }

        protected override void OnClear()
        {
            if (!pointatOutput.Equals(AnimationPlayableOutput.Null))
            {
                m_pointAtPlayable.DisconnectInput(0);
                m_Graph.Stop();
                m_Graph.DestroyOutput(pointatOutput);
                m_Graph.Play();
            }

            if (nativeArray.IsCreated)
            {
                nativeArray.Dispose();
            }
            pointatOutput = AnimationPlayableOutput.Null;
            oriOutput = AnimationPlayableOutput.Null;
            callBack = null;
            joints = null;
            endLookTime = 0;
            startLookTime = 0;
        }

        /// <summary>
        /// 外部调用清理
        /// </summary>
        public void CallClear()
        {
            OnClear();
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

        /// <summary>
        /// 轴向类型
        /// </summary>
        private enum Axis
        {
            Forward = 1,
            Back,
            Up,
            Down,
            Left,
            Right
        }

        /// <summary>
        /// 关节数据结构
        /// </summary>
        private class Joints
        {
            public Transform joint;
            public Axis axis = Axis.Up;
            public float msxAngle;
            public int weight;
        }
    }

    /// <summary>
    /// 指向目标类型
    /// </summary>
    public enum PointAtType
    {
        [Describe(name = "头部")]
        head,
        [Describe(name = "右手")]
        rHand,
        [Describe(name = "头和右手")]
        headAndrHand,
    }

    /// <summary>
    /// 指向目标身上的节点类型
    /// </summary>
    public enum TargetPoint
    {
        [Describe(name = "根节点")]
        root,
        [Describe(name = "头部(角色)")]
        head,
        [Describe(name = "右手挂点(角色)")]
        rHand
    }
}
