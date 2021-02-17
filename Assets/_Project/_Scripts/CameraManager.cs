using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31.MessageKit;
using DG.Tweening;

namespace FarmGame
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField]
        Vector3 topCameraPosition;
        [SerializeField]
        float topCameraSize = 10f;
        [SerializeField]
        float fieldCameraSize = 5f;

        [SerializeField]
        float cameraSwitchSpeed = 0.5f;
        Camera mainCamera;

        [SerializeField]
        Vector3 previousFieldCameraPosition = Vector3.zero;

        private void Start()
        {
            mainCamera = Camera.main;
            MessageKit.addObserver(Messages.SwitchToField, OnSwitchToField);
            MessageKit.addObserver(Messages.SwitchToTopView, OnSwitchToTopView);
        }

        void OnSwitchToTopView(){
            previousFieldCameraPosition = mainCamera.transform.position;
            mainCamera.transform.DOMove(topCameraPosition, cameraSwitchSpeed);
            mainCamera.DOOrthoSize(topCameraSize,cameraSwitchSpeed);
        }

        void OnSwitchToField(){
            mainCamera.transform.DOMove(previousFieldCameraPosition, cameraSwitchSpeed);
            mainCamera.DOOrthoSize(fieldCameraSize,cameraSwitchSpeed);
        }
    }

}