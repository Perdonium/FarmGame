using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31.MessageKit;
using DG.Tweening;
using UnityEngine.Experimental.Rendering.Universal;

namespace FarmGame
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField]
        Vector3 topCameraPosition;
        [SerializeField]
        float topCameraSize = 18f;
        [SerializeField]
        float fieldCameraSize = 5f;

        [SerializeField]
        float cameraSwitchSpeed = 0.5f;
        Camera mainCamera;

        [SerializeField]
        Vector3 previousFieldCameraPosition = Vector3.zero;

        PixelPerfectCamera pixelPerfectCamera;
        private void Start()
        {
            mainCamera = Camera.main;
            pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();

            MessageKit.addObserver(Messages.SwitchToField, OnSwitchToField);
            MessageKit.addObserver(Messages.SwitchToTopView, OnSwitchToTopView);
        }

        void OnSwitchToTopView(){
            previousFieldCameraPosition = mainCamera.transform.position;
            pixelPerfectCamera.enabled = false;
            mainCamera.transform.position = topCameraPosition;
            //mainCamera.transform.DOMove(topCameraPosition, cameraSwitchSpeed);
            mainCamera.DOOrthoSize(topCameraSize,cameraSwitchSpeed);
        }

        void OnSwitchToField(){
            pixelPerfectCamera.enabled = true;
            mainCamera.transform.position = previousFieldCameraPosition;
            //mainCamera.transform.DOMove(previousFieldCameraPosition, cameraSwitchSpeed).OnComplete(() => pixelPerfectCamera.enabled = true);
            mainCamera.DOOrthoSize(fieldCameraSize,cameraSwitchSpeed);
        }
    }

}