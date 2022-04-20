import timeit
import time
import events
import pandapower as pp
from pandapower import pandapowerNet, LoadflowNotConverged, OPFNotConverged
import warnings

warnings.simplefilter(action='ignore')

class PowerNetwork:
# The pandapower format network
    _net: pandapowerNet
    events: events.Events

    def __init__(self):
        self.events = events.Events()

# initializes the pandapower datastructure.
    def create_empty_network(self, **kwargs):
        print('Simulating Network Started. Please wait...')
        self._net = pp.create_empty_network(**kwargs)

# Adds one bus in table net[“bus”].
    def add_bus(self, **kwargs):
        pp.create_bus(self._net, **kwargs)

# Creates an external grid connection.
    def create_external_grid(self, **kwargs):
        pp.create_ext_grid(self._net, **kwargs)

    def create_generator(self, **kwargs):
        pp.create_gen(self._net, **kwargs)

    def create_static_generator(self, **kwargs):
        pp.create_sgen(self._net, **kwargs)

    def create_load(self, **kwargs):
        pp.create_load(self._net, **kwargs)

    def create_line(self, **kwargs):
        pp.create_line_from_parameters(self._net, **kwargs)

    def create_dc_line(self, **kwargs):
        pp.create_dcline(self._net, **kwargs)

    def create_poly_cost(self, **kwargs):
        pp.create_poly_cost(self._net, **kwargs)
        self._net.bus['max_vm_pu'] = 1.5
        self._net.line['max_loading_percent'] = 1000

    def run_opp(self):
        try:
            pp.create_poly_cost(self._net, 0, 'dcline', cp1_eur_per_mw=5)
            pp.runopp(self._net)
        except OPFNotConverged:
            print(' Error: Optimal Power Flow did not converge!')
            self.events.on_opf_error('Error: Optimal Power Flow did not converge!')
            return
            print(f'====== GENERATOR RESULT======')
            print(self._net.res_gen.p_mw)

# changes the node in panda dataframe
    def change_node(self, element: str, **kwargs):
        index: int = self.get_element_index(element, kwargs['name'])
        for key, value in kwargs.items():
            self._net[element][key][index] = value
        self.run_network()

    def change_dc_line(self, element: str, **kwargs):
        index: int = self.get_element_index(element, kwargs['name'])
        for key, value in kwargs.items():
            self._net[element][key][index] = value
        self.run_network()

    def change_ac_line(self, element: str, **kwargs):
        index: int = self.get_element_index(element, kwargs['name'])
        for key, value in kwargs.items():
            self._net[element][key][index] = value
        self.run_network()

# Returns the element(s) identified by a name or regex and its element-table.
    def get_element_index(self, element: str, name: str):
        for i in range(len(self._net[element]['name'])):
            if self._net[element]['name'][i] == name:
                return i
        return -1

    def replace(self, element: str, to_element: str, name: str):
        if element == "gen" and to_element == "sgen":
            index: int = self.get_element_index(element, name)
            pp.replace_gen_by_sgen(self._net, gens=[index], sgen_indices=None, cols_to_keep=None, add_cols_to_keep=None)
            self.run_network()
            print(f"======")
            print(f"{name} replaced from {element} to {to_element}")
            print(self._net[element])
            print(f"======")

# Runs a power flow
    def run_network(self):
        print('Power flow Calculation Started...')
        #start = timeit.timeit()
        start_time = time.time()
        try:
            pp.runpp(self._net)
        except LoadflowNotConverged:
            print("Error: Load flow Not Converged")
            self.events.on_pf_error("Error: Load flow Not Converged")
            return
        except UserWarning:
            print("Error: No reference bus is available. Either add an ext_grid or a gen with slack=True")
            self.events.on_slack_error("Error: No slack bus is available")
            return

        #end = timeit.timeit()
        print("=== Simulation took: {:2.4f} ms".format(1000 * (time.time() - start_time)))
        #print(f"=== Simulation took {(end - start)}")
       # print(self._net)
        print("=== BUS RESULT ===")
        print(self._net.res_bus)
        print("=== LINE RESULT ===")
        print(self._net.res_line[['p_to_mw', 'p_from_mw', 'pl_mw', 'loading_percent']])
        print("=== DC_LINE RESULT ===")
        print(self._net.res_dcline)
     #   print("=== res_gen.p_mw RESULT ===")
     #   print(self._net.res_gen.p_mw)
        self.events.on_bus_result(self.get_bus_result_json())
        self.events.on_line_result(self.get_line_result_json())
        self.events.on_dc_line_result(self.get_dc_line_result_json())

    def get_network(self):

        return self._net

# Saves the power flow bus results in JSON format
    def get_bus_result_json(self):
        df = self._net.res_bus.fillna(0)
        return df.to_json()

    def get_line_result_json(self):
        df = self._net.res_line.fillna(0)
        return df.to_json()

    def get_dc_line_result_json(self):
        df = self._net.res_dcline.fillna(0)
        return df.to_json()