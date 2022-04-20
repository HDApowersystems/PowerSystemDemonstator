using Network;

namespace PowerNetwork.View
{
    public class StaticGeneratorView : NodeView
    {
        public StaticGenerator StaticData;

        public override MessageType messageType { get => MessageType.create_static_generator; }

        public override Node node
        {
            get => StaticData;
            set => StaticData = (StaticGenerator)value;
        }
    }
}