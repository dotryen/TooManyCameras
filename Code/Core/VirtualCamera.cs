using System.Threading.Tasks;
using Sandbox.Utility;

namespace MANIFOLD.Camera {
    /// <summary>
    /// Imagine a <see cref="CameraComponent"/> but it isn't real.
    /// </summary>
    [EditorHandle("materials/gizmo/virtual_cam.png")]
    [Title(LibraryData.TITLE_SPLIT + "Virtual Camera"), Icon("videocam"), Category(LibraryData.CATEGORY)]
    public sealed class VirtualCamera : Component, Component.ExecuteInEditor {
        /// <summary>
        /// This camera's priority in the stack.
        /// </summary>
        [Property]
        public int Priority { get; set; }
        
        /// <summary>
        /// Camera's horizontal field of view in degrees.
        /// </summary>
        [Property, Range(0, 180), Header("Lens")]
        public float FieldOfView { get; set; } = 90f;
        
        /// <summary>
        /// Does this camera have an look target?
        /// If not, <see cref="TrackingTarget"/> is used.
        /// </summary>
        [Property, Header("Targets")]
        public bool UseLookTarget { get; set; }
        /// <summary>
        /// Target to track. Used as the <see cref="LookTarget"/> as well by default.
        /// </summary>
        [Property]
        public GameObject TrackingTarget { get; set; }
        /// <summary>
        /// Target to look at.
        /// </summary>
        [Property, ShowIf(nameof(UseLookTarget), true)]
        public GameObject LookTarget { get; set; }

        /// <summary>
        /// Should switching to this camera use a special transition?
        /// </summary>
        [Property, Header("Transition")]
        public bool UseCustomTransition { get; set; } = false;
        [ShowIf(nameof(UseCustomTransition), true)]
        [Property]
        public TransitionData TransitionData { get; set; }
        
        private CameraSystem internalSystem;
        private List<CameraExtension> extensions;
        private int lastComponentCount;
        
        public CameraSystem System {
            get {
                if (internalSystem == null) {
                    internalSystem = Scene.GetSystem<CameraSystem>();
                }
                return internalSystem;
            }
        }
        public bool IsActive => System.CurrentCamera == this;

        internal void OnSystemInit(CameraSystem system) {
            internalSystem = system;
            
            extensions = Components.GetAll<CameraExtension>(FindMode.EverythingInSelf).ToList();
            foreach (CameraExtension ext in extensions) {
                ext.Camera = this;
                ext.OnCameraInitialize();
            }
        }

        internal void DoExtensionUpdate(out Vector3 localPos, out Rotation localRot) {
            localPos = Vector3.Zero;
            localRot = Rotation.Identity;

            foreach (CameraExtension ext in extensions) {
                ext.OnCameraUpdate(ref localPos, ref localRot);
            }
        }

        protected override void OnEnabled() {
            System.ActivateCamera(this);
        }

        protected override void OnDisabled() {
            System.DeactivateCamera(this);
        }

        protected override void OnUpdate() {
            if (!Scene.IsEditor) return; // we probably dont need to check this at runtime right?
            if (lastComponentCount != Components.Count) {
                extensions = Components.GetAll<CameraExtension>(FindMode.EverythingInSelf).ToList();
                Log.Info("component count has changed");
                lastComponentCount = Components.Count;
            }
        }
        
        protected override void DrawGizmos() {
            if (IsActive) return;
            if (!System.Brain.IsValid()) return;
            CameraComponent camera = System.Brain.Camera;

            float horizontalAngle = FieldOfView;
            // THIS IS WRONG BUT GOOD ENOUGH
            float verticalAngle = horizontalAngle * (camera.ScreenRect.Height / camera.ScreenRect.Width); // cam fov is horizontal

            Vector3 origin = Vector3.Zero;
            Vector3 forward = Vector3.Forward;
            Ray tl = new Ray(origin, forward * new Angles(verticalAngle * 0.5f, horizontalAngle * 0.5f, 0f));
            Ray tr = new Ray(origin, forward * new Angles(verticalAngle * 0.5f, -horizontalAngle * 0.5f, 0f));
            Ray bl = new Ray(origin, forward * new Angles(-verticalAngle * 0.5f, horizontalAngle * 0.5f, 0f));
            Ray br = new Ray(origin, forward * new Angles(-verticalAngle * 0.5f, -horizontalAngle * 0.5f, 0f));
            
            Frustum frustum = Frustum.FromCorners(tl, tr, br, bl, camera.ZNear, camera.ZFar);

            Gizmo.Draw.Color = new Color(0.5f, 0f, 0f);
            Gizmo.Draw.LineFrustum(frustum);
        }
    }
}
