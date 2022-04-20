using Network;

namespace PowerNetwork.View
{
    public class ExternalGridView : NodeView
    {
        public override MessageType messageType { get => MessageType.create_external_grid; }

        public ExternalGrid ExternalGrid;
        public override Node node
        {
            get => ExternalGrid;
            set => ExternalGrid = (ExternalGrid)value;
        }
    }
}