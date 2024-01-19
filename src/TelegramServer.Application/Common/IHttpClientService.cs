using System.Threading.Tasks;

namespace TelegramServer.Common;

public interface IHttpClientService
{
    Task<T> PostAsync<T>(string url, object paramObj);
}