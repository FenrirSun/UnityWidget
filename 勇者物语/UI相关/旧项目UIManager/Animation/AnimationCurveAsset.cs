using UnityEngine;
using System.Collections;

namespace Code.External.Engine
{
    public class AnimationCurveAsset : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve curve;

        public AnimationCurveAsset(AnimationCurve curve)
        {
            this.curve = curve;
        }
        public AnimationCurve Curve
        {
            get { return curve; }
        }
    }
}