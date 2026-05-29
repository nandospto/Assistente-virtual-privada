using System.Net;
using System.Text;

class Program
{
    private static HttpListener? _listener;
    private const string URL = "http://localhost:5000/";

    static async Task Main(string[] args)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(URL);
        
        try
        {
            _listener.Start();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SERVIDOR] API Console iniciada em {URL}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar o servidor: {ex.Message}");
            return;
        }

        // Loop principal assíncrono para manter o servidor escutando
        while (true)
        {
            try
            {
                // Aguarda uma requisição HTTP chegar (Não bloqueia a aplicação)
                HttpListenerContext context = await _listener.GetContextAsync();
                
                // Despacha o processamento da requisição para outra thread (Multi-thread)
                _ = Task.Run(() => ProcessarRequisicaoAsync(context));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar conexão: {ex.Message}");
            }
        }
    }

    private static async Task ProcessarRequisicaoAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string metodo = request.HttpMethod; // GET, POST, etc.
        string rota = request.Url?.AbsolutePath ?? "/"; // /config, /posicao, etc.

        Console.WriteLine($"[REQUISIÇÃO] {metodo} chamado em {rota} às {DateTime.Now.ToLongTimeString()}");

        string respostaTexto = "";
        HttpStatusCode statusResultado = HttpStatusCode.OK;

        try
        {
            // ROTEAMENTO MANUAL: Simula o comportamento de Controllers de uma API
            if (metodo == "GET" && rota == "/config")
            {
                // Simulação de retorno de dados (Seria buscado no banco futuramente)
                respostaTexto = "{\"skin\": \"default\", \"volume\": 80}";
                response.ContentType = "application/json";
            }
            else if (metodo == "POST" && rota == "/posicao")
            {
                // Lendo o corpo do POST (o JSON enviado pelo cliente)
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string corpoJson = await reader.ReadToEndAsync();
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[DADO RECEBIDO]: {corpoJson}");
                    Console.ResetColor();
                }

                respostaTexto = "{\"status\": \"Posição salva com sucesso no Console!\"}";
                response.ContentType = "application/json";
                statusResultado = HttpStatusCode.Created;
            }
            else
            {
                // Rota não encontrada
                respostaTexto = "{\"erro\": \"Rota não encontrada\"}";
                response.ContentType = "application/json";
                statusResultado = HttpStatusCode.NotFound;
            }
        }
        catch (Exception ex)
        {
            respostaTexto = $"{{\"erro\": \"{ex.Message}\"}}";
            statusResultado = HttpStatusCode.InternalServerError;
        }
        finally
        {
            // Envia a resposta de volta para quem chamou (Godot ou Navegador)
            byte[] buffer = Encoding.UTF8.GetBytes(respostaTexto);
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)statusResultado;

            using (Stream output = response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
            }
            
            response.Close();
        }
    }
}