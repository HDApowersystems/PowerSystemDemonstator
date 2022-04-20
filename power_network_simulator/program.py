import asyncio
import signal
import sys

from async_network import network
from async_network.network import Peer
from power_network.client import Client


class Program:
    server: network.Server

    def __init__(self):
        signal.signal(signal.SIGINT, self.signal_handler)
        self.server = network.Server()
        asyncio.run(self.server.run(on_new_client=self.on_new_client))

    @staticmethod
    def on_new_client(peer: Peer):

        Client(peer)

    def signal_handler(self, inc_signal, frame):
        print(f'terminated by {inc_signal} ({frame})')
        self.server.close()

        sys.exit()


if __name__ == "__main__":
    print("Please wait for server to connect...")
    Program()
