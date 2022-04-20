using PowerNetwork;

namespace Network
{
    public enum MessageType
    {
        create_empty_network = 0,
        add_bus = 1,
        create_generator = 2,
        create_static_generator = 3,
        create_external_grid = 4,
        create_load = 5,
        create_line = 6,
        run_network = 7,
        close_connection = 8,
        node_net_changed = 9,
        create_dc_line = 10,
        dcline_net_changed = 11,
        acline_net_changed = 12,
        create_poly_cost = 13,
        run_opp = 14,

    }
    public class Packet
    {
        public MessageType message_type;
        public object data;
    }


    public enum ResultType
    {
        Line,
        Bus,
        PFError, //power flow error
        DcLine,
        SlackError, //slack error
        OPFError  //optimal power flow error
    }

    public class Result
    {
        public ResultType result_type;
        
    }

    public class BusDataFrameResult : Result
    {
        public BusDataFrame data;
    }

    public class LineDataFrameResult : Result
    {
        public LineDataFrame data;

    }
    public class DcLineDataFrameResult : Result
    {
        public DcLineDataFrame data;

    }

    public class PFErrorResult : Result
    {
        public string message;
    }

    public class SlackErrorResult : Result
    {
        public string message;
    }
    public class OPFErrorResult : Result
    {
        public string message;
    }

}
