import pandapower as pp
from pandapower import pandapowerNet


class PowerNetwork:
    net: pandapowerNet = pp.create_empty_network(name="Empty_Network")

    def __init__(self):
        self.net: pandapowerNet = pp.create_empty_network(name="Empty_Network")

    def create_empty_network(self, name="(Empty_Network"):
        net = pp.create_empty_network(name=name)

    def add_bus(self, index: int, name: str, voltage: int, in_service: bool):
        pp.create_bus(self.net, vn_kv=voltage, name=name, in_service=in_service, index=index)

    def create_external_grid(self, name: str, bus_index: int, voltage: float, degree: float, in_service: bool):
        pp.create_ext_grid(self.net, bus_index, vm_pu=voltage, va_degree=degree, name=name,
                           in_service=in_service)

    def create_generator(self, bus_index: int, name: str, voltage: float, power: float, min_power: float,
                         max_power: float, in_service: bool):
        pp.create_gen(self.net, bus_index, p_mw=power, vm_pu=voltage, name=name, min_p_mw=-min_power,
                      max_p_mw=max_power, in_service=in_service)

    def create_static_generator(self, bus_index: int, name: str, power: float, min_power: float,
                                max_power: float, in_service: bool, reactive_power: float):
        pp.create_sgen(self.net, bus_index, p_mw=power, q_mvar=reactive_power, name=name, min_p_mw=-min_power,
                       max_p_mw=max_power, in_service=in_service)

    def create_load(self, bus_index: int, name: str, power: float, reactive_power: float, in_service: bool):
        pp.create_load(self.net, bus_index, p_mw=power, name=name, in_service=in_service, q_mvar=reactive_power)

    def create_line(self, from_bus: int, to_bus: int, name: str, length: float, resistance: float, inductance: float,
                    capacitance: float, max_current: float):
        pp.create_line_from_parameters(self.net, from_bus=from_bus, to_bus=to_bus, length_km=length,
                                       r_ohm_per_km=resistance, x_ohm_per_km=inductance,
                                       c_nf_per_km=capacitance, max_i_ka=max_current, name=name)

    def get_bus(self):
        return self.net.bus

    def get_network(self):
        return self.net


network_instance: PowerNetwork = PowerNetwork()


def get_net_instance():
    global network_instance
    return network_instance


def init():
    network = get_net_instance()
    network.create_empty_network()
    network.add_bus(0, "Thermal", 400, True)
    network.add_bus(1, "External", 400, True)
    network.create_external_grid("Ext. Grid", 1, 1.1, 0, True)
    network.create_load(0, "internal_load", 100, 0.0005, True)
    network.create_line(0, 1, "Line_1", 10, 0, 0.0005, 0.000001, 0.4)
    network.print_network()
    return network


if __name__ == "__main__":
    init()

#
# # pp.create_line(net, from_bus=0, to_bus=1, length_km=30, std_type="NAYY 4x50 SE")
# # print(pp.available_std_types(net))
#
# l0 = pp.create_line_from_parameters(net, from_bus=0, to_bus=1, length_km=30, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.72, name="lineA")
# l1 = pp.create_line_from_parameters(net, from_bus=2, to_bus=1, length_km=30, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.58, name="lineB")
# l2 = pp.create_line_from_parameters(net, from_bus=1, to_bus=3, length_km=200, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.43, name="lineC")
# l3 = pp.create_line_from_parameters(net, from_bus=3, to_bus=4, length_km=100, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.29, name="lineD")
# l4 = pp.create_line_from_parameters(net, from_bus=3, to_bus=5, length_km=50, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.43, name="lineE")
# l5 = pp.create_line_from_parameters(net, from_bus=5, to_bus=6, length_km=50, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.58, name="lineF")
# l6 = pp.create_line_from_parameters(net, from_bus=5, to_bus=7, length_km=50, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.72, name="lineG")
# l7 = pp.create_line_from_parameters(net, from_bus=0, to_bus=9, length_km=100, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=1.15, name="lineH")
# l8 = pp.create_line_from_parameters(net, from_bus=7, to_bus=9, length_km=250, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=1.15, name="lineI")
# l9 = pp.create_line_from_parameters(net, from_bus=7, to_bus=8, length_km=50, r_ohm_per_km=0., x_ohm_per_km=0.005,
#                                     c_nf_per_km=0.00001, max_i_ka=0.14, name="lineJ")
# print(net.sgen)
#
# print("\nstart : to_excel")
# start = time.time()
# pp.runpp(net)
#
# pp.to_json(net, r'C:\Users\Sharareh\PycharmProjects\myGridCreation\myGrid.json')
#
# end = time.time()
# print(f"\npowerflow time: {end - start}")
#
# print("\nstart : Grid 02")
# start = time.time()
#
# pp.runpp(net)
#
# end = time.time()
# print(f"\npowerflow time: {end - start}")
#
# start = time.time()
#
# pp.runpp(net)
#
# end = time.time()
# print(f"\npowerflow time: {end - start}")
#
# df = net.gen
# start = time.time()
# df['p_mw'] = df['p_mw'].replace([350.0], 700)
#
# pp.runpp(net)
#
# end = time.time()
# print(f"\npowerflow time: {end - start}")
#
# # print("\nGen: ")
# # print(net.gen)
# # print("\nLoad: ")
# print(net.load)
# print("")
# # print(net.res_bus)
# # print(net.res_line.loading_percent)
# # df =net.res_line.loading_percent
# # df.to_csv(r'C:\Users\Sharareh\PycharmProjects\DataPath\DataExeclSheets\line_loading.csv')
#
# # print(net.gen.iloc[0][2])
