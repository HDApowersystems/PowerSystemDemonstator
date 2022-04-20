using System.Collections.Generic;
using System;
using Newtonsoft.Json;


namespace PowerNetwork
{

    [Serializable]
    public class Bus
    {
        public int index;
        public string name;
        public float vn_kv;
        public bool in_service = true;

        [JsonIgnore]
        public BusResult BusResult = new BusResult();

    }

    [Serializable]
    public class NetworkData
    {
        public string name;
    }

    [Serializable]
    public class Node
    {
        public int bus;    
        public bool in_service;
        public string name;
        //public string zone;
        [JsonIgnore]
        public virtual string elementID { get; }
    }
    [Serializable]
    public class Generator : Node
    {
        public float p_mw;
        public float max_p_mw;
        public float min_p_mw;
        public float vm_pu;
        public bool slack = false;

        public override string elementID => "gen";
        public Generator()
        {
        }
    }

    [Serializable]
    public class PolyCost
    {
        
     //   [JsonIgnore]
      //  public int index;
       public int element;
        public string et = "gen";
        public float cp1_eur_per_mw;
    
    }

    [Serializable]
    public class StaticGenerator : Node
    {
        public float p_mw;
        public float q_mvar;
        public override string elementID => "sgen";
        public StaticGenerator()
        {
        }
    }

  

    [Serializable]
    public class ExternalGrid : Node
    {
        public float va_degree;
        public float vm_pu;
        public override string elementID => "ext_grid"; 
        public ExternalGrid()
        {
        }
    }

    [Serializable]
    public class Load : Node
    {
        public float p_mw;
        public float q_mvar;
        public override string elementID => "load";
        public Load()
        {
        }
    }

    [Serializable]
    public class Line
    {
       
        public bool in_service = true;
        public float length_km;
        [JsonIgnore]
        public int index;
        public int from_bus;
        public int to_bus;

        public float c_nf_per_km= 11;
        public float r_ohm_per_km= 0.059f;
        public float x_ohm_per_km= 0.253f;

        public float max_i_ka;
        public string name;
        [JsonIgnore]
        public string elementID => "line";

    }

    [Serializable]
    public class DcLine
    {
        public bool in_service;
        public float p_mw;
        public float loss_mw;
        public float loss_percent;
        public float vm_from_pu;
        public float vm_to_pu;
        [JsonIgnore]
        public int index;
        public int from_bus;
        public int to_bus;
        public string name;

        // public float max_p_mw;
        [JsonIgnore]
        public string elementID => "dcline";       
    }

    public class BusDataFrame
    {
        public Dictionary<int, float> vm_pu;
        public Dictionary<int, float> va_degree;
        public Dictionary<int, float> p_mw;
        public Dictionary<int, float> q_mvar;
    }

    public class LineDataFrame
    {
        public Dictionary<int, float> p_from_mw;
        public Dictionary<int, float> q_from_mvar;
        public Dictionary<int, float> p_to_mw;
        public Dictionary<int, float> q_to_mvar;
        public Dictionary<int, float> pl_mw;
        public Dictionary<int, float> ql_mvar;
        public Dictionary<int, float> i_from_ka;
        public Dictionary<int, float> i_to_ka;
        public Dictionary<int, float> i_ka;
        public Dictionary<int, float> vm_from_pu;
        public Dictionary<int, float> va_from_degree;
        public Dictionary<int, float> vm_to_pu;
        public Dictionary<int, float> va_to_degree;
        public Dictionary<int, float> loading_percent;
    }
    public class DcLineDataFrame
    {
        public Dictionary<int, float> p_from_mw;
        public Dictionary<int, float> q_from_mvar;
        public Dictionary<int, float> p_to_mw;
        public Dictionary<int, float> q_to_mvar;
        public Dictionary<int, float> pl_mw;
        public Dictionary<int, float> vm_from_pu;
        public Dictionary<int, float> va_from_degree;
        public Dictionary<int, float> vm_to_pu;
        public Dictionary<int, float> va_to_degree;
    }

    [Serializable]
    public class BusResult
    {
        public Action<float> OnBusVmChanged;
       public Action<float> OnBusVdegChanged;

      //  public int bus; 
        public float vm_pu;
        public float va_degree;
        public float BusVmag
        {
            get => vm_pu;
            set
            {
                if (value != vm_pu)
                    OnBusVmChanged?.Invoke(value);
                vm_pu = value;
            }
        }
      
        public float BusVdeg
        {
            get => va_degree;
            set
            {
                if(value != va_degree)
                    OnBusVdegChanged? .Invoke(value);
                va_degree = value;
            }
        }
        public float p_mw;
        public float q_mvar;
    }

    [Serializable]
    public class LineResult
    {
        public Action<float> OnLineLoadingChanged;

        public Func<float, float> OnLinePfromChanged;
        public Func<float, float> OnLineQfromChanged;
        public Func<float, float> OnLinePtoChanged;
        public Func<float, float> OnLineQtoChanged;

        public float p_from_mw;
        public float q_from_mvar;
        public float p_to_mw;
        public float q_to_mvar;

        public float pl_mw;
        public float ql_mvar;
        public float i_from_ka;
        public float i_to_ka;
        public float i_ka;
        public float vm_from_pu;
        public float va_from_degree;
        public float vm_to_pu;
        public float va_to_degree;
        public float loading_percent;
       
        public float LinePfrom
        {
            get => p_from_mw;
            set
            {
                if (value != p_from_mw)
                    OnLinePfromChanged?.Invoke(value);
                p_from_mw = value;
            }
        }

        public float LineQfrom
        {
            get => q_from_mvar;
            set
            {
                if (value != q_from_mvar)
                    OnLineQfromChanged?.Invoke(value);
                q_from_mvar = value;
            }
        }

        public float LinePto
        {
            get => q_to_mvar;
            set
            {
                if(value != q_to_mvar)    
                    OnLinePtoChanged?.Invoke(value);
                q_to_mvar = value;
            }
        }

        public float LineQto
        {
            get => p_to_mw;
            set
            {
                if (value != p_to_mw)
                    OnLineQtoChanged?.Invoke(value);
                p_to_mw = value;
            }
        }
        public float LineLoad
        {
            get => loading_percent;
            set
            {
                if (value != loading_percent)
                    OnLineLoadingChanged?.Invoke(value);
                loading_percent = value;
            }
        }
    }

    [Serializable]
    public class DcLineResult
    {
        public Action<float> OnDcLinePlChanged;

        public Func<float, float> OnDcLinePfromChanged;
        public Func<float, float> OnDcLineQfromChanged;
        public Func<float, float> OnDcLinePtoChanged;
        public Func<float, float> OnDcLineQtoChanged;

        public float p_from_mw;
        public float q_from_mvar;
        public float p_to_mw;
        public float q_to_mvar;
        public float pl_mw;
        /*  public float vm_from_pu;
            public float va_from_degree;
            public float vm_to_pu;
            public float va_to_degree;
           */

        public float DcLinePfrom
        {
            get => p_from_mw;
            set
            {
                if (value != p_from_mw)
                    OnDcLinePfromChanged?.Invoke(value);
                p_from_mw = value;
            }
        }

        public float DcLineQfrom
        {
            get => q_from_mvar;
            set
            {
                if (value != q_from_mvar)
                    OnDcLineQfromChanged?.Invoke(value);
                q_from_mvar = value;
            }
        }

        public float DcLinePto
        {
            get => q_to_mvar;
            set
            {
                if (value != q_to_mvar)
                    OnDcLinePtoChanged?.Invoke(value);
                q_to_mvar = value;
            }
        }

        public float DcLineQto
        {
            get => p_to_mw;
            set
            {
                if (value != p_to_mw)
                    OnDcLineQtoChanged?.Invoke(value);
                p_to_mw = value;
            }
        }

        public float DcLinePl
        {
            get => pl_mw;
            set
            {
                if (value != pl_mw)
                    OnDcLinePlChanged?.Invoke(value);
                pl_mw = value;
            }
        }

    }

    public class NodeNetChange
    {
        public string element;
        public Node node;
    }

    public class DcLineNetChange
    {
        public string element;
        public DcLine dcline;
    }

    public class AcLineNetChange
    {
        public string element;
        public Line acline;
    }

}
