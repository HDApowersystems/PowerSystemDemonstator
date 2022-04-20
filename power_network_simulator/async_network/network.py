import asyncio
import logging
import sys
from asyncio import StreamReader, StreamWriter
from typing import Coroutine
from events import Events


class Peer:
    writer: StreamWriter
    reader: StreamReader
    events = Events()
    can_read: bool

    def __init__(self, reader: StreamReader, writer: StreamWriter, server_event: Events):
        self.handle_read(reader)
        self.writer = writer
        self.reader = reader
        self.can_read = True
        server_event.on_close += self.on_server_close

    def on_server_close(self):
        self.close()

# Fires when there is data to be read
    def handle_read(self, reader: StreamReader):
        asyncio.create_task(self.read_loop(reader))

    async def read_loop(self, reader: StreamReader):
        while self.can_read:
            received_bytes = await reader.readline()
            if len(received_bytes) < 1:
                continue
            json = received_bytes.decode()
            self.events.on_received(json)

    def write(self, json: str):
        asyncio.Task(self.write_async(json))

    async def write_async(self, jsn: str):
        #print('sending: ' + jsn)
        data_bytes = bytearray(jsn, 'utf-8')
        size = len(data_bytes)
        #print(f"Sending: {size!r}")
        size_bytes = int.to_bytes(size, byteorder=sys.byteorder, length=4)
        size_bytearray = bytearray(size_bytes)
        buffer = size_bytearray + data_bytes
        self.writer.write(buffer)
        await self.writer.drain()

    def close(self):
        self.can_read = False
        peer_name = self.writer.get_extra_info('peername')
        print(f"closing peer: {peer_name}")
        self.writer.close()

class Server:
    log = logging.getLogger(__name__)
    loop: Coroutine
    events: Events
    def __init__(self):
        self.events = Events()
    # create and configure main logger
    log = logging.getLogger(__name__)
    loop: Coroutine = None
    events: Events()

    def handle_client(self, reader: StreamReader, writer: StreamWriter):
        peer = Peer(reader, writer, self.events)
        return peer

    async def accept_client(self, reader: StreamReader, writer: StreamWriter, on_new_client):
        peer = self.handle_client(reader, writer)
        peer_name = writer.get_extra_info('peername')
        print(f'client connected: {peer_name}')
        self.log.info("")
        on_new_client(peer)

    def close(self):
        print("closing server...")
        self.events.on_net_close()
        # for task in asyncio.Task.all_tasks():
        #     task.cancel()
        for t in asyncio.tasks.all_tasks():
            t.cancel()

    async def run(self, on_new_client):
        server = await asyncio.start_server(
            lambda reader, writer: self.accept_client(reader, writer, on_new_client), '127.0.0.1', 8858)
        addrs = ', '.join(str(sock.getsockname()) for sock in server.sockets)
        print(f'Serving on {addrs}')
        self.loop = asyncio.get_event_loop()
        async with server:
             await server.serve_forever()

        # async with server:
        #     global loop = await server.serve_forever()
