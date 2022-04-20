using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace PowerNetwork.View
{
    public class ErrorPupop : MonoBehaviour
    {
        public GameObject uiObject;
        public NetworkManager networkManager;


        private void Awake()
        {

            uiObject.SetActive(false);
            networkManager.OnErrorRecieved = OnErrorRecieved;

        }
        private void Update()
        {
            
        }
        private void OnErrorRecieved()
        {
            uiObject.SetActive(true);
            StartCoroutine("WaitForSec");

        }

        IEnumerable WaitForSec()
        {
            yield return new WaitForSeconds(10);
            Destroy(uiObject);
            Destroy(gameObject);

        }
    }

}
