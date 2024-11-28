using System.Collections.Generic;
using Sandbox;
using Sandbox.Utility;

namespace MANIFOLD.Camera {
    /// <summary>
    /// The actual brain of the camera system.
    /// </summary>
    public sealed class CameraSystem : GameObjectSystem {
        private CameraBrain mainCameraBrain;
        private bool cameraStackDirty;
        private LinkedList<VirtualCamera> cameraStack;

        private VirtualCamera transitionFrom;
        private VirtualCamera transitionTo;

        private bool inTransition;
        private bool reverseTransition;
        private float currentTransitionTimer;
        private float currentTransitionElapsed;
        private TransitionData currentTransitionData;

        public CameraBrain Brain => mainCameraBrain;
        public VirtualCamera LastCamera => transitionFrom;
        public VirtualCamera CurrentCamera => transitionTo;
        
        public CameraSystem(Scene scene) : base(scene) {
            Listen(Stage.SceneLoaded, 100, OnLoad, "cameras.load");
            Listen(Stage.Interpolation, 100, CameraUpdate, "camera.update");
            cameraStack = new LinkedList<VirtualCamera>();
        }

        private void OnLoad() {
            FindBrain();

            foreach (VirtualCamera cam in Scene.Components.GetAll<VirtualCamera>(FindMode.EverythingInDescendants)) {
                cam.OnSystemInit(this);
            }
        }
        
        private void CameraUpdate() {
            if (!FindBrain()) return;
            
            if (Scene.IsEditor) {
                if (!mainCameraBrain.UpdateInEditor) {
                    return;
                }
            }

            if (cameraStackDirty) {
                OnStackEdit();
            }

            if (!transitionTo.IsValid()) return;
            GetCameraTransform(transitionTo, out Vector3 toPos, out Rotation toRot);
            
            if (!Scene.IsEditor && inTransition) {
                float linearFactor = currentTransitionTimer / currentTransitionData.Duration;
                float easedFactor = 0f;

                {
                    float offset = currentTransitionElapsed / currentTransitionData.Duration;
                    float evalTime = linearFactor.Remap(offset, 1);

                    if (currentTransitionData.Mode == TransitionMode.Predefined) {
                        var func = Easing.GetFunction(currentTransitionData.EaseFunction.ToString());
                        easedFactor = func(evalTime) + currentTransitionElapsed;
                    } else if (currentTransitionData.Mode == TransitionMode.Curve) {
                        easedFactor = currentTransitionData.EaseCurve.Evaluate(evalTime);
                    }

                    easedFactor = easedFactor.Remap(0, 1, offset);
                }

                GetCameraTransform(transitionFrom, out Vector3 fromPos, out Rotation fromRot);
                mainCameraBrain.WorldPosition = Vector3.Lerp(fromPos, toPos, easedFactor);
                mainCameraBrain.WorldRotation = Rotation.Slerp(fromRot, toRot, easedFactor);
                mainCameraBrain.Camera.FieldOfView = MathX.Lerp(transitionFrom.FieldOfView, transitionTo.FieldOfView, easedFactor);

                currentTransitionTimer += Time.Delta;
                if (currentTransitionTimer >= currentTransitionData.Duration) {
                    currentTransitionTimer = currentTransitionData.Duration;
                    inTransition = false;
                }
            } else {
                mainCameraBrain.WorldPosition = toPos;
                mainCameraBrain.WorldRotation = toRot;
                mainCameraBrain.Camera.FieldOfView = transitionTo.FieldOfView;
            }
        }

        private void GetCameraTransform(VirtualCamera cam, out Vector3 pos, out Rotation rot) {
            cam.DoExtensionUpdate(out Vector3 localPos, out Rotation localRot);
            pos = cam.WorldPosition + (localPos * cam.WorldRotation);
            rot = cam.WorldRotation * localRot;
        }
        
        public void ActivateCamera(VirtualCamera newCamera, bool updateNow = false) {
            if (!mainCameraBrain.IsValid()) {
                Log.Warning($"Tried to activate virtual camera '${newCamera.GameObject.Name}' but there is no brain in the scene.");
                return;
            }
            
            VirtualCamera highestCamera = null;
            foreach (VirtualCamera camera in cameraStack) {
                if (camera.Priority <= newCamera.Priority) {
                    highestCamera = camera;
                }
            }

            if (highestCamera != null) {
                cameraStack.AddAfter(cameraStack.Find(highestCamera), newCamera);
            } else {
                cameraStack.AddLast(newCamera);
            }

            
            cameraStackDirty = true;
            if (updateNow) {
                OnStackEdit();
            }
        }

        public void DeactivateCamera(VirtualCamera camera, bool updateNow = false) {
            cameraStack.Remove(camera);
            cameraStackDirty = true;
            if (updateNow) {
                OnStackEdit();
            }
        }

        private bool FindBrain() {
            if (mainCameraBrain.IsValid()) return true;
            mainCameraBrain = Scene.Components.GetInDescendants<CameraBrain>();
            return mainCameraBrain.IsValid();
        }
        
        private void OnStackEdit() {
            var lastNode = cameraStack.Last;
            if (lastNode == null) return;

            // var previousNode = lastNode.Previous;
            // if (previousNode != null) {
            //     transitionFrom = previousNode.Value;
            // }

            VirtualCamera newTo = lastNode.Value;
            if (newTo != transitionTo && transitionTo.IsValid()) {
                TransitionData newData = newTo.UseCustomTransition ? newTo.TransitionData : mainCameraBrain.TransitionData;

                if (newData.Mode != TransitionMode.Cut) {
                    inTransition = true;

                    if (newTo == transitionFrom) {
                        float elapsedNorm = 1 - (currentTransitionTimer / currentTransitionData.Duration);
                        currentTransitionTimer = newData.Duration * elapsedNorm;
                        currentTransitionElapsed = newData.AbsoluteEase ? 0f : newData.Duration * elapsedNorm;
                    } else {
                        currentTransitionTimer = 0;
                        currentTransitionElapsed = 0;
                    }
                }
                currentTransitionData = newData;
                transitionFrom = transitionTo;
            }
            transitionTo = newTo;
            cameraStackDirty = false;
        }
    }
}
