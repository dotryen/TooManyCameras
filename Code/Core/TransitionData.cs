using System;
using Sandbox;
using Sandbox.Utility;

namespace MANIFOLD.Camera {
    public enum TransitionMode { Cut, Predefined, Curve }
    public enum PredifinedEase { Linear, EaseIn, EaseOut, EaseInOut, SineEaseIn, SineEaseOut, SineEaseInOut, QuadraticIn, QuadraticOut, QuadraticInOut, ExpoIn, ExpoOut, ExpoInOut, BounceIn, BounceOut, BounceInOut }

    [Serializable]
    public sealed class TransitionData {
        public TransitionMode Mode { get; set; } = TransitionMode.Predefined;
        [HideIf(nameof(Mode), TransitionMode.Cut)]
        public float Duration { get; set; } = 1f;
        /// <summary>
        /// Should the ease be applied in an absolute manner?
        /// </summary>
        [Space, HideIf(nameof(Mode), TransitionMode.Cut)]
        public bool AbsoluteEase { get; set; } = false;
        [ShowIf(nameof(Mode), TransitionMode.Predefined)]
        public PredifinedEase EaseFunction { get; set; } = PredifinedEase.EaseOut;
        [ShowIf(nameof(Mode), TransitionMode.Curve)]
        public Curve EaseCurve { get; set; }
    }
}
