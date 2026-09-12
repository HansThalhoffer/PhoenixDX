using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixDX.Helper {
    /// <summary>
    /// Kleiner HTTP-Server, der an einem angegebenen Port bindet und
    /// zur Laufzeit erzeugtes XML unter dem Root-Pfad liefert.
    /// </summary>
    public class XmlStateServer {
        private static readonly ConcurrentQueue<Func<string>> _xmlGenerator = new ConcurrentQueue<Func<string>>();
        private static HttpListener _listener;
        private static Task _listenTask;
        private static CancellationTokenSource _internalCts;
        private static bool _isRunning;

        public XmlStateServer(Func<string> xmlGenerator) {
            _xmlGenerator.Enqueue(xmlGenerator);
        }


        static XmlStateServer() {
            try {
                Start(8080);
                Application.ApplicationExit += (s, e) => Stop();
                _xmlGenerator.Enqueue(new Func<string>(() => BaseInfo()));
                _xmlGenerator.Enqueue(new Func<string>(() => AnmeldungsInfo.AlsXml()));
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Fehler beim Starten des XmlStateServer: {ex.Message}");
            }
        }

        private static string BaseInfo() {
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<XmlStateServer><State>");
            sb.Append("<Timestamp>" + DateTime.UtcNow.ToString("o") + "</Timestamp>");

            sb.Append("<OSVersion>" + System.Runtime.InteropServices.RuntimeInformation.OSDescription + "</OSVersion>");

            sb.Append("<OSArchitecture>" + System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString() + "</OSArchitecture>");
            sb.Append("<CPUCount>" + Environment.ProcessorCount + "</CPUCount>");
            sb.Append("<CPUArchitecture>" + System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString() + "</CPUArchitecture>");
            long workingSet = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            sb.Append("<ProcessWorkingSetBytes>" + workingSet + "</ProcessWorkingSetBytes>");

            long available = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            sb.Append("<GCAvailableMemoryBytes>" + available + "</GCAvailableMemoryBytes>");

            sb.Append("</State>");

            return sb.ToString();
        }

        /// <summary>
        /// Startet den Server auf dem angegebenen Port. Bindet an 127.0.0.1.
        /// Liefert unter "/" das XML mit Content-Type "application/xml".
        /// </summary>
        /// <param name="port">Port, an den gebunden wird.</param>
        /// <param name="externalToken">Externer CancellationToken zum Stoppen (optional).</param>
        private static void Start(int port, CancellationToken externalToken = default) {
            if (_isRunning)
                return;

            string prefix = $"http://127.0.0.1:{port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            CancellationToken token = _internalCts.Token;

            try {
                _listener.Start();
            }
            catch (HttpListenerException hex) {
                _listener.Close();
                _listener = null;
                throw new InvalidOperationException($"HttpListener konnte Prefix {prefix} nicht binden. Fehlermeldung: {hex.Message}", hex);
            }

            _isRunning = true;
            _listenTask = Task.Run(async () => {
                while (!token.IsCancellationRequested) {
                    HttpListenerContext context = null;
                    try {
                        context = await _listener.GetContextAsync().ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException) {
                        break;
                    }
                    catch (HttpListenerException) {
                        break;
                    }
                    catch (Exception) {
                        // swallow individual accept errors and continue
                        continue;
                    }

                    // Handle request in fire-and-forget to keep accept loop responsive
                    _ = Task.Run(async () => {
                        try {
                            HttpListenerRequest request = context.Request;
                            HttpListenerResponse response = context.Response;

                            StringBuilder sb = new StringBuilder();
                            foreach (var generator in _xmlGenerator) {
                                try {
                                    sb.Append(generator());
                                }
                                catch {
                                    // ignore individual generator errors
                                }
                            }
                            sb.Append("</XmlStateServer>");


                            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

                            response.ContentType = "application/xml; charset=utf-8";
                            response.ContentEncoding = Encoding.UTF8;
                            response.ContentLength64 = buffer.Length;
                            response.StatusCode = (int)HttpStatusCode.OK;

                            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                            response.OutputStream.Close();
                        }
                        catch {
                            try {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.OutputStream.Close();
                            }
                            catch {
                                // ignore
                            }
                        }
                    }, token);
                }
            }, token);
        }

        /// <summary>
        /// Stoppt den Server und wartet auf Beendigung.
        /// </summary>
        public static void Stop() {
            if (!_isRunning)
                return;

            try {
                _internalCts?.Cancel();
                _listener?.Stop();
            }
            finally {
                try {
                    _listenTask?.Wait(2000);
                }
                catch {
                    // ignore
                }

                _listener?.Close();
                _listener = null;
                _listenTask = null;
                _internalCts?.Dispose();
                _internalCts = null;
                _isRunning = false;
            }
        }
    }
}
