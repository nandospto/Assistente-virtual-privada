using System.Net;
using System.Text;
using Npgsql;
using Microsoft.Extensions.Configuration;

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
        
        // 1. Carrega os dados de user do DB local
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        Console.WriteLine(config);
        // 2. Recupera String
        string? connectionString = config["ConnectionStrings:DefaultConnection"];
        Console.WriteLine($"String de conexão: {connectionString}");

        // 3. Valida
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("[ERRO] Não encontrada a string nos segredos locais.");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentType = "application/json";
            string errorJson = "{\"erro\": \"String de conexão não configurada.\"}";
            byte[] errorBuffer = Encoding.UTF8.GetBytes(errorJson);
            response.ContentLength64 = errorBuffer.Length;
            await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
            return;
        }


        string metodo = request.HttpMethod; // GET, POST, etc.
        string rota = request.Url?.AbsolutePath ?? "/"; // /config, /posicao, etc.

        Console.WriteLine($"[REQUISIÇÃO] {metodo} chamado em {rota} às {DateTime.Now.ToLongTimeString()}");

        string respostaTexto = "";
        HttpStatusCode statusResultado = HttpStatusCode.OK;

        try
        {
            // ROTEAMENTO MANUAL: Simula o comportamento de Controllers de uma API
            // === GERENCIAMENTO DA CONFIGURAÇÃO ===
            if (metodo == "GET" && rota == "/config")
            {
                int id = 0;
                int posicao_x = 0;
                int posicao_y = 0;
                string skinDoBanco = "default";
                int volumeDoBanco = 80;

                // 2. Abre a conexão e executa o comando SQL
                using (var conexao = new NpgsqlConnection(connectionString))
                {
                    conexao.Open();
                    string sql = "SELECT id, posicao_x, posicao_y, skin_atual, volume_voz FROM configuracao LIMIT 1;";

                    using (var comando = new NpgsqlCommand(sql, conexao))
                    using (var leitor = comando.ExecuteReader())
                    {
                        if (leitor.Read()) // Se encontrou a linha de configuração
                        {
                            // Recupera os valores das colunas correspondentes
                            id = leitor.GetInt32(0);
                            posicao_x = leitor.GetInt32(1);
                            posicao_y = leitor.GetInt32(2);
                            skinDoBanco = leitor.GetString(3);
                            volumeDoBanco = leitor.GetInt32(4);
                        }
                    }
                } // O 'using' garante que a conexão com o banco é FECHADA aqui

                // 3. Monta o JSON dinâmico com os dados reais do banco
                string jsonResposta = $"{{id\": {id}, \"posicao_x\": {posicao_x}, \"posicao_y\": {posicao_y}, skin\": \"{skinDoBanco}\", \"volume\": {volumeDoBanco}}}";

                // 4. Fluxo de envio HTTP (O mesmo algoritmo que você fez funcionar)
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResposta);
                context.Response.ContentLength64 = buffer.Length;

                System.IO.Stream fluxoSaida = context.Response.OutputStream;
                fluxoSaida.Write(buffer, 0, buffer.Length);
                fluxoSaida.Close();
            }
            else if (metodo == "PUT" && rota == "/posicao") { /* UPDATE na tabela Configuracao */ }

            // === GERENCIAMENTO DOS CHATS (INTEGRA COM GEMINI) ===
            else if (metodo == "GET" && rota == "/chats") { /* SELECT na tabela Chat (Lista as abas) */ }
            else if (metodo == "POST" && rota == "/chats") { /* INSERT na tabela Chat (Cria aba nova) */ }
            else if (metodo == "POST" && rota == "/mensagens") { /* INSERT no Historico + Chamada ao Gemini */ }

            // === GERENCIAMENTO DAS NOTAS (LOCAL/ISOLADO) ===
            else if (metodo == "GET" && rota == "/notas") { /* SELECT na tabela Notas (Lista lembretes) */ }
            else if (metodo == "POST" && rota == "/notas") { /* INSERT na tabela Notas (Salva lembrete) */ }
            else if (metodo == "DELETE" && rota == "/notas") { /* DELETE na tabela Notas (Apaga lembrete) */ }
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