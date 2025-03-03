using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace SETHD.FantasySnake.Camera
{
    public class CameraController
    {
        private readonly ICinemachineCamera mainCamera;
        private readonly UnityEngine.Camera sceneCamera;
        
        [Inject]
        public CameraController(ICinemachineCamera mainCamera, UnityEngine.Camera sceneCamera)
        {
            this.mainCamera = mainCamera;
            this.sceneCamera = sceneCamera;
        }
        
        public void Focus(GameObject target)
        {
            mainCamera.LookAt = target.transform;
        }

        public Ray CastRayToMousePosition()
        {
            return sceneCamera.ScreenPointToRay(Mouse.current.position.value);
        }
    }
}
