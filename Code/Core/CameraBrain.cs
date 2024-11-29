using Sandbox;

namespace MANIFOLD.Camera {
    [Title(LibraryData.TITLE_SPLIT + "Brain"), Icon("videocam"), Category(LibraryData.CATEGORY)]
    public sealed class CameraBrain : Component {
        [Property, RequireComponent, Hide] public CameraComponent Camera { get; set; }

        [Property]
        public bool UseRealTime { get; set; } = true;
        [Property]
        public bool UpdateInEditor { get; set; } = true;
        [Property, Title("Default Transition")]
        public TransitionData TransitionData { get; set; }
    }
}
