using Network;
using System;

namespace PowerNetwork.View
{
    public class GeneratorView : NodeView
    {
        public override MessageType messageType { get => MessageType.create_generator; }
        public Generator GeneratorData;
        public override Node node
        {
            get => GeneratorData;
            set => GeneratorData = (Generator)value;
        }


     /*   protected override void OnReplaced(string toElement)
        {
            base.OnReplaced(toElement);
            StaticGeneratorView sgenView = gameObject.AddComponent<StaticGeneratorView>();
            sgenView.StaticData = new StaticGenerator();
            sgenView.StaticData.bus = GeneratorData.bus;
            sgenView.StaticData.name = GeneratorData.name;
            sgenView.StaticData.in_service = GeneratorData.in_service;
            Destroy(this);
        }*/


    }
}