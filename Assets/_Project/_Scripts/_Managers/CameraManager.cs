using UnityEngine;
using Prime31.MessageKit;
using DG.Tweening;
using UnityEngine.Experimental.Rendering.Universal;

namespace FarmGame
{
    //Manage the camera transitions
    public class CameraManager : MonoBehaviour
    {

        #region Properties
        Camera mainCamera;

        [SerializeField]
        Vector3 topCameraPosition;
        float topCameraSize; //Determined by the starting orthographic size because I use MatchWidth


        [SerializeField]
        float fieldCameraSize = 5f;

        [SerializeField]
        float cameraSwitchSpeed = 0.5f;

        [SerializeField]
        Vector3 previousFieldCameraPosition = Vector3.zero; //Useful to go back to previous position on field switch

        MatchWidth matchWidth;

        [SerializeField]
        BoxCollider2D cameraBounds;

        #endregion

        private void Start()
        {
            mainCamera = Camera.main;
            topCameraSize = mainCamera.orthographicSize;

            matchWidth = mainCamera.GetComponent<MatchWidth>();

            MessageKit.addObserver(Messages.SwitchToFieldView, OnSwitchToFieldView);
            MessageKit.addObserver(Messages.SwitchToTopView, OnSwitchToTopView);
            MessageKit.addObserver(Messages.CameraMoved, CheckBoundsCamera);
        }

        //Switch the camera to the top view setup
        void OnSwitchToTopView(){
            previousFieldCameraPosition = mainCamera.transform.position;

            mainCamera.transform.DOMove(topCameraPosition, cameraSwitchSpeed).OnComplete(() => matchWidth.enabled = true);
            mainCamera.DOOrthoSize(topCameraSize,cameraSwitchSpeed);
        }

        //Switch the camera to the field view setup
        void OnSwitchToFieldView(){
            matchWidth.enabled = false;
            mainCamera.transform.DOMove(previousFieldCameraPosition, cameraSwitchSpeed);
            mainCamera.DOOrthoSize(fieldCameraSize,cameraSwitchSpeed);
        }

        //Check that the camera is within space
        void CheckBoundsCamera()
        {
            Vector2 boundsCenter = (Vector2)cameraBounds.transform.position + cameraBounds.offset;
            Vector3 cameraPosition = mainCamera.transform.position;

            float height = 2f * mainCamera.orthographicSize;
            float width = height * mainCamera.aspect;
            cameraPosition.x = Mathf.Clamp(cameraPosition.x, boundsCenter.x - cameraBounds.size.x / 2 + width / 2, boundsCenter.x + cameraBounds.size.x / 2 - width / 2);
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, boundsCenter.y - cameraBounds.size.y / 2 + height / 2, boundsCenter.y + cameraBounds.size.y / 2 - height / 2);

            mainCamera.transform.position = cameraPosition;
        }
    }

}