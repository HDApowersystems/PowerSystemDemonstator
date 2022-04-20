using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerNetwork;
using Network;
using PowerNetwork.View;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;

public class NetworkManager : MonoBehaviour
{
    public NetworkData NetworkData;

    public ResetScript ResetScriptpf;
    public SlackResetScript ResetScriptSlack;
    
    private Stopwatch stopwatch;
    private TcpPeer tcpPeer;

   
    private List<NodeView> nodeViews = new List<NodeView>();
    private List<LineView> lineViews = new List<LineView>();
    private List<DcLineView> dclineViews = new List<DcLineView>();
  //  private List<PolyCostView> polyCostViews = new List<PolyCostView>();

    private Dictionary<int, BusView> BusViewDic = new Dictionary<int, BusView>();
    private Dictionary<int, LineView> LineViewDict = new Dictionary<int, LineView>();
    private Dictionary<int, DcLineView> DclineViewDict = new Dictionary<int, DcLineView>();
  //  private Dictionary<int, PolyCostView> PolyCostViewDict = new Dictionary<int, PolyCostView>();
   

    public  Action OnErrorRecieved;
    public Action OnSlackErrorRecieved;

    #region start
    private void Start()
    {
        tcpPeer = new TcpPeer();
        tcpPeer.Connect(System.Net.IPAddress.Loopback, 8858);
        tcpPeer.OnConnected += TcpPeer_OnConnected;
        tcpPeer.OnResult += TcpPeer_OnResultReceived;

        nodeViews.AddRange(FindObjectsOfType<NodeView>());
        nodeViews.Sort((n1, n2) => n1.BusView.Bus.index.CompareTo(n2.BusView.Bus.index));

        lineViews.AddRange(FindObjectsOfType<LineView>());
        lineViews.Sort((l1, l2) => l1.Line.index.CompareTo(l2.Line.index));

        dclineViews.AddRange(FindObjectsOfType<DcLineView>());
        dclineViews.Sort((l1, l2) => l1.dcLine.index.CompareTo(l2.dcLine.index));

      //  polyCostViews.AddRange(FindObjectsOfType<PolyCostView>());
       // polyCostViews.Sort((p1, p2) => p1.polyCost.element.CompareTo(p2.polyCost.element));

        BusView[] busViews = FindObjectsOfType<BusView>();
    //    PolyCostView pol= FindObjectOfType<PolyCostView>();

        for (int i = 0; i < busViews.Length; i++)
        {
            if (BusViewDic.ContainsKey(busViews[i].Bus.index) == false) BusViewDic.Add(busViews[i].Bus.index, busViews[i]);
            else Debug.LogError($"A Bus with the same ID already exists: {busViews[i].gameObject}", busViews[i]);
        }
       
        
        for (int i = 0; i < nodeViews.Count; i++)
        {
            NodeView nodeView = nodeViews[i];
            nodeView.OnNodeChanged = OnNodeChanged;           
        }

        for (int i = 0; i < lineViews.Count; i++)
        {
            if (LineViewDict.ContainsKey(lineViews[i].Line.index) == false)
            { LineViewDict.Add(lineViews[i].Line.index, lineViews[i]); }
            else
            {
                Debug.LogError($"A line with the same Index already exists: {lineViews[i].gameObject}", lineViews[i]);
            }
            LineView acVie = lineViews[i];
            acVie.OnAClineChanged = OnAClineChanged;
        }

        for (int i = 0; i < dclineViews.Count ; i++)
        {
            if (DclineViewDict.ContainsKey(dclineViews[i].dcLine.index) == false) DclineViewDict.Add(dclineViews[i].dcLine.index, dclineViews[i]); 
            else
            {
                Debug.LogError($"A dc line with the same Index already exists: {dclineViews[i].gameObject}", dclineViews[i]);
            }
            DcLineView dcVie = dclineViews[i];
            dcVie.OnDClineChanged = OnDClineChanged;
        }

       /* for (int i = 0; i < polyCostViews.Count - 1; i++)
        {
            PolyCostViewDict.Add(polyCostViews[i].polyCost.element, polyCostViews[i]);
        }*/
    }
    #endregion


    private void OnDisable()
    {
        SendMessage(MessageType.close_connection, string.Empty);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            stopwatch = Stopwatch.StartNew();
            SendMessage(MessageType.run_network, string.Empty);
        }
       
    }

    private void TcpPeer_OnConnected()
    {
        Debug.Log("TcpPeer Connected");
        stopwatch = Stopwatch.StartNew();


        SendMessage(MessageType.create_empty_network, NetworkData);

        foreach (var bus in BusViewDic)
        {
            SendMessage(MessageType.add_bus, bus.Value.Bus);
        }

        for (int n = 0; n < nodeViews.Count; n++)
        {
            SendMessage(nodeViews[n]);
        }
        for (int i = 0; i < lineViews.Count; i++)
        {
            SendMessage(lineViews[i]);
        }
        for (int i = 0; i < dclineViews.Count; i++)
        {
            SendMessage(dclineViews[i]);
        }

        SendMessage(MessageType.run_network, string.Empty);
       /* Loom.QueueOnMainThread(() => SendMessage(MessageType.create_poly_cost, polyCostData0));        
        /* for (int j = 0; j < polyCostViews.Count; j++)
         {           
             Loom.QueueOnMainThread(() => SendMessage(MessageType.create_poly_cost, polyCostView));
            //SendMessage(polyCostViews[j]);
         }*/

       // Loom.QueueOnMainThread(() => SendMessage(MessageType.run_opp, string.Empty));
    }

    private void OnNodeChanged(Node node)
    {
        NodeNetChange nd = new NodeNetChange();
        nd.element = node.elementID;
        nd.node = node;
        SendMessage(MessageType.node_net_changed, nd);
    }
    private void OnAClineChanged(Line line)
    {
        AcLineNetChange acl = new AcLineNetChange();
        acl.element = line.elementID;
        acl.acline = line;
        SendMessage(MessageType.acline_net_changed, acl);
    }

    private void OnDClineChanged(DcLine dcline)
    {
        DcLineNetChange dcl = new DcLineNetChange();
        dcl.element = dcline.elementID;
        dcl.dcline = dcline;
        SendMessage(MessageType.dcline_net_changed, dcl);
    }



    #region send
    private void SendMessage(DcLineView dcLineView)
    {
        SendMessage(MessageType.create_dc_line, dcLineView.dcLine);
    }
    private void SendMessage(PolyCostView polyCostView)
    {
       // Loom.QueueOnMainThread(() => SendMessage(MessageType.net_create_poly_cost, polyCostView));
        SendMessage(MessageType.create_poly_cost, polyCostView);
    }
    public void SendMessage(NodeView nodeView)
    {

        if (nodeView.node != null) SendMessage(nodeView.messageType, nodeView.node);
    }

    public void SendMessage(LineView lineView)
    {
        SendMessage(MessageType.create_line, lineView.Line);
    }

    private void SendMessage(MessageType messageType, object data)
    {
        Packet packet = new Packet();
        packet.message_type = messageType;
        packet.data = data;
        tcpPeer.Send(packet);
    }
    #endregion
    private void OnDestroy()
    {
        tcpPeer?.Disconnect();
    }
    #region recieve
    private void TcpPeer_OnResultReceived(Result result)
    {
        switch (result)
        {
            case BusDataFrameResult busResult:
                UpdateBusResults(busResult);
                break;
            case LineDataFrameResult lineResult:
                UpdateLineResults(lineResult);
                break;

            case DcLineDataFrameResult dclineResult:
                UpdateDcLineResults(dclineResult);
                break;

            case PFErrorResult PFerrorResult:
                Debug.Log("pf error");
                Loom.QueueOnMainThread(() => ResetScriptpf.SetUpReset());
                break;

            case SlackErrorResult SlackerrorResult:
                Debug.Log("slack error");
                Loom.QueueOnMainThread(() => ResetScriptSlack.SetUpReset());
                break;


                /* case OPFErrorResult OPFerrorResult:
                     Loom.QueueOnMainThread(() => ResetScriptOpf.SetUpReset());
                     break;*/
        }
        stopwatch.Stop();
     //   print($"Elpased Calc Time: {stopwatch.Elapsed.TotalMilliseconds}");

    }
    #endregion

    #region Update Results

    private void UpdateDcLineResults(DcLineDataFrameResult dclineResult)
    {
        DcLineDataFrame dclineDataFrame = dclineResult.data;

        for (int i = 0; i < dclineDataFrame.p_from_mw.Count; i++)
        {
            DcLineResult dclineRes = DclineViewDict[i].dcLineResult;

            dclineRes.p_from_mw= dclineDataFrame.p_from_mw[i];
            dclineRes.q_from_mvar= dclineDataFrame.q_from_mvar[i];
            dclineRes.p_to_mw= dclineDataFrame.p_to_mw[i];  
            dclineRes.q_to_mvar= dclineDataFrame.q_to_mvar[i];

            float lpfrom = dclineDataFrame.p_from_mw[i];
            Loom.QueueOnMainThread(() => dclineRes.DcLinePfrom = lpfrom);

            float lqfrom = dclineDataFrame.q_from_mvar[i];
            Loom.QueueOnMainThread(() => dclineRes.DcLineQfrom = lqfrom);

            float lpto = dclineDataFrame.p_to_mw[i];
            Loom.QueueOnMainThread(() => dclineRes.DcLinePto = lpto);

            float lqto = dclineDataFrame.q_to_mvar[i];
            Loom.QueueOnMainThread(() => dclineRes.DcLineQto = lqto);

            float pl = dclineDataFrame.pl_mw[i];
            Loom.QueueOnMainThread(() => dclineRes.DcLinePl = pl);
            /*  dclineRes.pl_mw = dclineDataFrame.pl_mw[i];
              dclineRes.vm_from_pu= dclineDataFrame.vm_from_pu[i];
              dclineRes.va_from_degree= dclineDataFrame.va_from_degree[i];
              dclineRes.vm_to_pu= dclineDataFrame.vm_to_pu[i];
              dclineRes.va_to_degree= dclineDataFrame.va_to_degree[i];
             for (int i = 0; i < polyCostViews.Count; i++)
          {
              Loom.QueueOnMainThread(() => SendMessage(polyCostViews[i]));
          }
  */

        }
    }

    private void UpdateLineResults(LineDataFrameResult lineResult)
    {
        LineDataFrame lineDataFrame = lineResult.data;
        for (int i = 0; i < lineDataFrame.i_from_ka.Count; i++)
        {
            LineResult lineRes = LineViewDict[i].LineResult;

            lineRes.pl_mw = lineDataFrame.pl_mw[i];
            lineRes.ql_mvar = lineDataFrame.ql_mvar[i];
            lineRes.i_from_ka = lineDataFrame.i_from_ka[i];
            lineRes.i_to_ka = lineDataFrame.i_to_ka[i];
            lineRes.i_ka = lineDataFrame.i_ka[i];
            lineRes.vm_from_pu = lineDataFrame.vm_from_pu[i];
            lineRes.va_from_degree = lineDataFrame.va_from_degree[i];
            lineRes.vm_to_pu = lineDataFrame.vm_to_pu[i];
            lineRes.va_to_degree = lineDataFrame.va_to_degree[i];

            float lpfrom = lineDataFrame.p_from_mw[i];
            Loom.QueueOnMainThread(() => lineRes.LinePfrom = lpfrom);

            float lqfrom = lineDataFrame.q_from_mvar[i];
            Loom.QueueOnMainThread(() => lineRes.LineQfrom = lqfrom);

            float lpto = lineDataFrame.p_to_mw[i];
            Loom.QueueOnMainThread(() => lineRes.LinePto = lpto);

            float lqto = lineDataFrame.q_to_mvar[i];
            Loom.QueueOnMainThread(() => lineRes.LineQto = lqto);

            float loadper = lineDataFrame.loading_percent[i];
            Loom.QueueOnMainThread(() => lineRes.LineLoad = loadper);
        }
    }

    private void UpdateBusResults(BusDataFrameResult busResult)
    {
        BusDataFrame busDataFrame = busResult.data;
        for (int i = 0; i < busDataFrame.p_mw.Count; i++)
        {
            BusResult res = BusViewDic[i].Bus.BusResult;
          //  res.p_mw = busDataFrame.p_mw[i];
          //  res.q_mvar = busDataFrame.q_mvar[i];
          //  res.va_degree = busDataFrame.va_degree[i];
         //   res.vm_pu = busDataFrame.vm_pu[i];
           float vdegree = busDataFrame.va_degree[i];
            Loom.QueueOnMainThread(() => res.BusVdeg = vdegree);

            float vmpu = busDataFrame.vm_pu[i];
           Loom.QueueOnMainThread(() => res.BusVmag = vmpu);
        }
    }
    #endregion

}
