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
        float topCameraSize;
        [SerializeField]
        float fieldCameraSize = 5f;

        [SerializeField]
        float cameraSwitchSpeed = 0.5f;
        Camera mainCamera;

        [SerializeField]
        Vector3 previousFieldCameraPosition = Vector3.zero;

        PixelPerfectCamera pixelPerfectCamera;
        MatchWidth matchWidth;

        private void Start()
        {
            mainCamera = Camera.main;
            topCameraSize = mainCamera.orthographicSize;

            pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
            matchWidth = mainCamera.GetComponent<MatchWidth>();

            MessageKit.addObserver(Messages.SwitchToFieldView, OnSwitchToField);
            MessageKit.addObserver(Messages.SwitchToTopView, OnSwitchToTopView);
        }

        void OnSwitchToTopView(){
            previousFieldCameraPosition = mainCamera.transform.position;
            /*
            pixelPerfectCamera.enabled = false;
            mainCamera.transform.position = topCameraPosition;
            */
            mainCamera.transform.DOMove(topCameraPosition, cameraSwitchSpeed).OnComplete(() => matchWidth.enabled = true);
            mainCamera.DOOrthoSize(topCameraSize,cameraSwitchSpeed);
        }

        void OnSwitchToField(){
            /*
            pixelPerfectCamera.enabled = true;
            mainCamera.transform.position = previousFieldCameraPosition;
            */
            matchWidth.enabled = false;
            mainCamera.transform.DOMove(previousFieldCameraPosition, cameraSwitchSpeed);
            mainCamera.DOOrthoSize(fieldCameraSize,cameraSwitchSpeed);
        }
    }

}