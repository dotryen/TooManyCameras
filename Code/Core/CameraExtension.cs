namespace MANIFOLD.Camera {
    /// <summary>
    /// An extension that modifies the camera when updated.
    /// </summary>
    [Icon("extension")]
    public abstract class CameraExtension : Component, Component.ExecuteInEditor {
        [Property, RequireComponent, Hide]
        public VirtualCamera Camera { get; set; }

        /// <summary>
        /// Called when the <see cref="VirtualCamera"/> is initialized. (Happens whether the GameObject is active or not)
        /// </summary>
        protected internal abstract void OnCameraInitialize();
        /// <summary>
        /// Called when the <see cref="VirtualCamera"/> is active.
        /// </summary>
        /// <remarks>Local transformations do not persist.</remarks>
        /// <param name="localPosition">Position local to the camera transform.</param>
        /// <param name="localRotation">Rotation local to the camera transform.</param>
        protected internal abstract void OnCameraUpdate(ref Vector3 localPosition, ref Rotation localRotation);
    }
}
