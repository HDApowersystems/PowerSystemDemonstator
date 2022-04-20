using Network;

namespace PowerNetwork.View
{
    public class LoadView : NodeView
    {
        public override MessageType messageType { get => MessageType.create_load; }
        public Load Load;
        public override Node node
        {
            get => Load;
            set => Load = (Load)value;
        }
    }
}