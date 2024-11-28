namespace MANIFOLD.Camera {
    /// <summary>
    /// Offsets the camera in local space. Does not persist.
    /// </summary>
    [Title(LibraryData.TITLE_SPLIT + "Local Offset"), Category(LibraryData.CATEGORY), Icon("flip_camera_android")]
    public sealed class CameraLocalOffset : CameraExtension {
        [Property]
        public Vector3 Offset { get; set; } = Vector3.Zero;
        [Property]
        public Angles AngularOffset { get; set; } = Angles.Zero;
        
        protected internal override void OnCameraInitialize() {
            
        }
        
        protected internal override void OnCameraUpdate(ref Vector3 localPosition, ref Rotation localRotation) {
            localPosition = Offset;
            localRotation = AngularOffset;
        }
    }
}
