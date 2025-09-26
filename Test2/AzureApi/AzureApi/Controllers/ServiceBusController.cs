using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AzureApi.Controllers
{
    [ApiController]
    [Route("servicebus")]
    public class ServiceBusController : ControllerBase
    {
        // TODO: Agregar Endpoint correcto
        private readonly string _connectionString = "";
        private readonly string _queueName = "devdemoqueue";

        [HttpPost("send")]
        public async Task<IActionResult> Send()
        {
            var sender = new ServiceBusSenderExample(_connectionString, _queueName);
            await sender.SendMessagesAsync();
            return Ok("Mensajes enviados");
        }

        [HttpGet("receive")]
        public async Task<IActionResult> Receive()
        {
            var receiver = new ServiceBusReceiverExample(_connectionString, _queueName);
            await receiver.ReceiveMessagesAsync();
            return Ok("Recepción finalizada (ver consola)");
        }
    }
}
