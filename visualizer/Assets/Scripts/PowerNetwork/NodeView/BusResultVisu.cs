using DG.Tweening;
using Network;
using System;
using UnityEngine;

namespace PowerNetwork.View
{
    public class BusResultVisu : MonoBehaviour
    {
        public BusResult BusResult;
        private Vector3 _initialScale;
        private void Awake()
        {
           // BusResult.OnBusVmChanged = OnBusVmChanged;
        }

        private void  OnBusVmChanged(float vmpu)
        {
            print($"{name} load changed to {vmpu}");
            float height = _initialScale.z * (vmpu *3);
            transform.DOScaleZ(height, 2f);
            float Width = _initialScale.z * (vmpu *3);
            transform.DOScaleZ(height, 2f);
        }
    }

   
}

