using Network;
using System;
using UnityEngine;

namespace PowerNetwork.View
{
    public class NodeView : MonoBehaviour
    {
        public virtual MessageType messageType { get => MessageType.add_bus; }
        public virtual Node node { get; set; }
        public BusView BusView;

        public Action<Node> OnNodeChanged;

        public virtual void Awake()
        {
            if (BusView == null)
            {
                BusView = transform.GetComponentInParent<BusView>();
               // BusView.Bus.BusResult.bus = BusView.Bus.index;
            }
        }

        public void NotifyChange()
        {
            OnNodeChanged?.Invoke(node);
        }

     
    }
}
