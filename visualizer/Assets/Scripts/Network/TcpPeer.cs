using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerNetwork;

namespace Network
{
    public class TcpPeer
    {
        JsonSerializerSettings JsonSetting = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.None,
        };

        #region Events
        public event Action OnConnected;
        public event Action<NetError> OnConnectError;
        public event Action OnSendFinished;
        public event Action OnDisconnect;
        public event Action<Result> OnResult;
        #endregion

        #region Private
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected;
        private CancellationTokenSource _readCanclation;
        #endregion

        #region Constructor

        public TcpPeer(TcpClient client = default)
        {
            if (client == default) client = new TcpClient();
            _client = client;
            _isConnected = false;
            _readCanclation = new CancellationTokenSource();
        }
        #endregion

        #region Connect
        public void Connect(IPAddress ip, int port)
        {
            _isConnected = false;

            Task.Factory.StartNew(() => ConnectAsync(ip, port));
        }

        public void Disconnect()
        {
            _readCanclation.Cancel();
            Close();
            OnDisconnect?.Invoke();
        }
        private async Task ConnectAsync(IPAddress ip, int port, CancellationToken cancellationToken = default)
        {

            if (_isConnected)
            {
                await CloseAsync();
                _client = new TcpClient();
            }
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine($"Connecting to {ip}:{port}");
            try
            {
                await _client.ConnectAsync(ip, port);
            }
            catch (Exception e)
            {
                Log.LogException(e);
                OnConnectError?.Invoke(NetError.CantStablishConnection);
                throw;
            }
            await CloseIfCanceled(cancellationToken);

            _stream = _client.GetStream();
            _isConnected = true;
            OnConnected?.Invoke();

            _ = Task.Factory.StartNew(() =>
                    Receive(cancellationToken),
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

        }

        private async Task CloseAsync()
        {
            await Task.Yield();
            Close();
        }

        private void Close()
        {

            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
        private async Task CloseIfCanceled(CancellationToken token, Action onClosed = null)
        {
            if (token.IsCancellationRequested)
            {
                await CloseAsync();
                onClosed?.Invoke();
                token.ThrowIfCancellationRequested();
            }
        }
        #endregion

        #region Send
        public void Send(Packet message)
        {
            // Serializes the specified object to a JSON string
            string json = JsonConvert.SerializeObject(message, JsonSetting);
            json += "\n";
            Log.Info($"Sending: {json}");
            // Encods bytes to a string
            byte[] bytes = Encoding.UTF8.GetBytes(json);
           // Log.Info($"Sending {bytes.Length} bytes");
            WriteBytes(bytes);
        }

        private async void WriteBytes(byte[] bytes, CancellationToken cancelToken = default)
        {
            // Asynchronously writes a sequence of bytes to the current stream
            await _stream.WriteAsync(bytes, 0, bytes.Length, cancelToken)
                .ContinueWith(task =>
                 {
                     if (task.IsCompleted)
                     {
                         Console.WriteLine($"{bytes.Length} bytes sent!");

                         OnSendFinished?.Invoke();
                     }
                 }, cancelToken);
        }
        #endregion

        #region Receive
        private async Task Receive(CancellationToken cancelToken = default)
        {
           // Console.WriteLine($"Start Receiving");
            if (cancelToken == default)
                cancelToken = _readCanclation.Token;
            while (_isConnected)
            {
                await CloseIfCanceled(cancelToken);
                byte[] sizeBytes = await ReadBytes(4, cancelToken);
                int size;
                try
                {
                    size = BitConverter.ToInt32(sizeBytes, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await CloseAsync();
                    throw;
                }
                Log.Info($"New Message incoming: {size} bytes");
                // Reads a sequence of bytes 
                byte[] packetBytes = await ReadBytes(size, cancelToken);
                // Encods bytes to a string
                string json = Encoding.UTF8.GetString(packetBytes, 0, packetBytes.Length);
               Log.Info($"Recived json: {json}");
                // Parse string to JSON object 
                JObject jobj = JObject.Parse(json);
                // Parse JSON object to JSON token
                JToken token = jobj["result_type"];
                Result result = default;
                if (token != null)
                {

                    if (int.TryParse(token.ToString(), out int type))
                    {

                        switch ((ResultType)type)
                        {
                            case ResultType.Line:
                                try
                                {
                                    LineDataFrameResult lineResult = new LineDataFrameResult();
                                    lineResult.result_type = ResultType.Line;
                                    string df_json = jobj["data"].ToString();
                                    lineResult.data = JsonConvert.DeserializeObject<LineDataFrame>(df_json);
                                    result = lineResult;
                                }
                                catch (Exception e)
                                {
                                    Log.LogException(e);
                                    throw;
                                }
                                break;

                            case ResultType.DcLine:
                                try
                                {
                                    DcLineDataFrameResult dclineResult = new DcLineDataFrameResult();
                                    dclineResult.result_type = ResultType.Line;
                                    string df_json = jobj["data"].ToString();
                                    dclineResult.data = JsonConvert.DeserializeObject<DcLineDataFrame>(df_json);
                                    result = dclineResult;
                                }
                                catch (Exception e)
                                {
                                    Log.LogException(e);
                                    throw;
                                }
                                break;

                            case ResultType.Bus:
                                BusDataFrameResult busResult = default;
                                try
                                {
                                    busResult = new BusDataFrameResult();
                                    busResult.result_type = ResultType.Bus;
                                    string busdf_json = jobj["data"].ToString();
                                    busResult.data = JsonConvert.DeserializeObject<BusDataFrame>(busdf_json);
                                }
                                catch (Exception e)
                                {
                                    Log.LogException(e);

                                    throw;
                                }
                                result = busResult;
                                break;

                            case ResultType.PFError:
                                PFErrorResult pfErrorResult = new PFErrorResult();
                                pfErrorResult.result_type = ResultType.PFError;
                                pfErrorResult.message = jobj["data"].ToString();
                                result = pfErrorResult;
                                break;
                            case ResultType.OPFError:
                                OPFErrorResult opfErrorResult = new OPFErrorResult();
                                opfErrorResult.result_type = ResultType.OPFError;
                                opfErrorResult.message = jobj["data"].ToString();
                                result = opfErrorResult;
                                break;

                            case ResultType.SlackError:
                                SlackErrorResult slackErrorResult = new SlackErrorResult();                               
                                slackErrorResult.result_type = ResultType.SlackError;
                                slackErrorResult.message = jobj["data"].ToString();
                                result = slackErrorResult;
                                break;


                        }
                    }
                    OnResult?.Invoke(result);


                }
            }
        }


        private async Task<byte[]> ReadBytes(int amount, CancellationToken cancelToken = default)
        {
            Log.Info($"Reading: {amount} bytes");

            if (amount == 0) return new byte[0];

            try
            {
                var tcs = new TaskCompletionSource<int>();

                cancelToken.Register(() => tcs.SetResult(default(int)));

                if (cancelToken.IsCancellationRequested == false)
                {

                    await CloseIfCanceled(cancelToken);
                    Task<byte[]> messageTask = ReadAsync(cancelToken, amount);
                    await Task.WhenAny(messageTask, tcs.Task);

                    if (messageTask.IsCompleted)
                    {
                        Log.Info($"Received: {messageTask.Result.Length} bytes");

                        return messageTask.Result;
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogException(e);
                throw;
            }

            return default;
        }

        private async Task<byte[]> ReadAsync(CancellationToken cancelToken, int amount)
        {
            int receivedBytes = 0;
            byte[] receiveBuffer = new byte[amount];

            while (receivedBytes < amount)
            {
                int remaining = amount - receivedBytes;

                receivedBytes = await _stream.ReadAsync(receiveBuffer, receivedBytes, remaining, cancelToken).ConfigureAwait(false);
            }
            if (receivedBytes != amount) return default;

            return receiveBuffer;
        }
        #endregion
    }

    public enum NetError
    {
        CantStablishConnection = 1,
    }
}
