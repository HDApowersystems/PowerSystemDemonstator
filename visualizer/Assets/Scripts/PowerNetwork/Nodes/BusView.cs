using DG.Tweening;
using System;
using UnityEngine;

namespace PowerNetwork.View
{   
    public class BusView : MonoBehaviour
    {
        public Bus Bus;
        private Vector3 _initialScale;
      
        private void Awake()
        {
        //  Bus.BusResult.OnBusVmChanged = OnBusVmChanged;
          Bus.BusResult.OnBusVdegChanged = OnBusVdegChanged;
        }

        private void OnBusVdegChanged(float v)
        {
            if (v < 0) { v = -v; }
            v /= 10;
            _initialScale = Vector3.one;
          //  float x = _initialScale.x * (v);
           // transform.DOScaleX(x, 3f);

            float y = _initialScale.y * (v*2 );
            transform.DOScaleY(y, 3f);
           
           // float z = _initialScale.z * (v );
           // transform.DOScaleZ(z, 3f);
        }

       private void OnBusVmChanged(float vmpu)
        {
            _initialScale = Vector3.one;
           // vmpu *= 3;
            float y = _initialScale.y * (vmpu*6 );
            transform.DOScaleY(y, 8f);
            float x= _initialScale.x * (vmpu*6f );
           transform.DOScaleX(x, 8f);
            float z = _initialScale.z * (vmpu*1f);
            transform.DOScaleZ(z, 8f);
        }
    }
}
