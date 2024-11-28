namespace MANIFOLD.Camera {
    /// <summary>
    /// Locks the <see cref="VirtualCamera"/> to it's <see cref="VirtualCamera.TrackingTarget"/>.
    /// </summary>
    [Title(LibraryData.TITLE_SPLIT + "Hard Lock"), Category(LibraryData.CATEGORY), Icon("lock")]
    public sealed class CameraHardLock : CameraExtension {
        public enum OrientMode {
            /// <summary>
            /// The target's orientation is not taken into account.
            /// </summary>
            None,
            /// <summary>
            /// The target's orientation only affects the position.
            /// </summary>
            [Title("Position Only")]
            NoRotation,
            /// <summary>
            /// The target's orientation affects both position and rotation.
            /// </summary>
            [Title("Position and Rotation")]
            Full
        }

        /// <summary>
        /// How should the Target's orientation be handled?
        /// </summary>
        [Property, Title("Orientation Handling")]
        public OrientMode Orient { get; set; } = OrientMode.Full;

        private GameObject lastTarget;
        private Vector3 targetBindRelativePos;
        private Vector3 targetBindLocalPos;
        private Rotation targetBindLocalRot;
        
        protected internal override void OnCameraInitialize() {
            if (!Camera.TrackingTarget.IsValid()) return;
            BindValues();
        }

        protected internal override void OnCameraUpdate(ref Vector3 localPosition, ref Rotation localRotation) {
            if (!Camera.TrackingTarget.IsValid()) return;
            if (Camera.TrackingTarget != lastTarget) {
                BindValues();
            }
            
            GameObject target = Camera.TrackingTarget;
            switch (Orient) {
                case OrientMode.None: {
                    Camera.WorldPosition = target.WorldPosition + targetBindRelativePos;
                    break;
                }
                case OrientMode.NoRotation: {
                    Camera.WorldPosition = target.WorldPosition + (targetBindLocalPos * target.WorldRotation);
                    break;
                }
                case OrientMode.Full: {
                    Camera.WorldPosition = target.WorldPosition + (targetBindLocalPos * target.WorldRotation);
                    Camera.WorldRotation = target.WorldRotation * targetBindLocalRot;
                    break;
                }
            }
        }

        private void BindValues() {
            lastTarget = Camera.TrackingTarget;
            targetBindRelativePos = WorldPosition - Camera.TrackingTarget.WorldPosition;
            
            Rotation inverse = Camera.TrackingTarget.WorldRotation.Inverse;
            targetBindLocalRot = WorldRotation * inverse;
            targetBindLocalPos = (WorldPosition - Camera.TrackingTarget.WorldPosition) * inverse;
        }
    }
}
