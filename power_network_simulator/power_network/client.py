import json

from async_network.network import Peer
from power_network.power_network import PowerNetwork

LINE_RESULT = 0
BUS_RESULT = 1
PF_ERROR_RESULT = 2
DC_LINE_RESULT = 3
SLACK_ERROR_RESULT = 4
OPF_ERROR_RESULT = 5



class Result:
    result_type: int
    data: str


class Client:
    peer: Peer
    net: PowerNetwork
    commands: dict

    def __init__(self, peer: Peer):
        self.peer = peer
        self.net = PowerNetwork()
        self.peer.events.on_received += self.on_received
        self.net.events.on_bus_result += self.on_bus_result
        self.net.events.on_line_result += self.on_line_result
        self.net.events.on_pf_error += self.on_net_pf_error
        self.net.events.on_slack_error += self.on_net_slack_error
        self.net.events.on_dc_line_result += self.on_dc_line_result
        self.net.events.on_opf_error += self.on_net_opf_error
        self.commands = {
            0: self.net.create_empty_network,
            1: self.net.add_bus,
            2: self.net.create_generator,
            3: self.net.create_static_generator,
            4: self.net.create_external_grid,
            5: self.net.create_load,
            6: self.net.create_line,
            7: self.net.run_network,
            8: self.close,
            9: self.net_node_changed,
            10: self.net.create_dc_line,
            11: self.net_dc_line_changed,
            12: self.net_ac_line_changed,
            13: self.net.create_poly_cost,
            14: self.net.run_opp,
        }

    def on_net_pf_error(self, msg: str):
        result = Result()
        result.result_type = PF_ERROR_RESULT
        result.data = msg
        self.send_message(result)

    def on_net_slack_error(self, msg: str):
            result = Result()
            result.result_type = SLACK_ERROR_RESULT
            result.data = msg
            self.send_message(result)

    def on_net_opf_error(self, msg: str):
        result = Result()
        result.result_type = OPF_ERROR_RESULT
        result.data = msg
        self.send_message(result)

    def net_replaced(self, **kwargs):
        self.net.replace(kwargs['element'], kwargs['toElement'], kwargs['name'])

    def net_node_changed(self, **kwargs):
        node = kwargs['node']
        self.net.change_node(kwargs['element'], **node)

    def net_dc_line_changed(self, **kwargs):
        dcline = kwargs['dcline']
        self.net.change_dc_line(kwargs['element'], **dcline)

    def net_ac_line_changed(self, **kwargs):
        acline = kwargs['acline']
        self.net.change_ac_line(kwargs['element'], **acline)

# Converts from json to python object
    def on_received(self, packet_json: str):
        jsn = json.loads(packet_json)
        #print(packet_json)
        index: int = jsn['message_type']
        if 'data' in jsn and jsn['data'] is not None and jsn['data'] != '':
            self.commands[index](**jsn['data'])
        else:
            self.commands[index]()

# Converts the power flow results to a bus result type
    def on_bus_result(self, result_json: str):
        result = Result()
        result.result_type = BUS_RESULT
        result.data = result_json
        self.send_message(result)

    def on_line_result(self, result_json: str):
        result = Result()
        result.result_type = LINE_RESULT
        result.data = result_json
        self.send_message(result)

    def on_dc_line_result(self, result_json: str):
        result = Result()
        result.result_type = DC_LINE_RESULT
        result.data = result_json
        self.send_message(result)

# sends the power flow bus results to the peer
    def send_message(self, result: Result):
        jsn = json.dumps(result.__dict__)
        self.peer.write(jsn)

    def close(self):
        self.peer.events.on_received -= self.on_received
        self.net.events.on_bus_result -= self.on_bus_result
        self.net.events.on_line_result -= self.on_line_result
        self.peer.close()
        self.net = None
        self.commands = None
