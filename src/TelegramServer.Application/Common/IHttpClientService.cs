using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace TelegramServer.Common;

public interface IHttpClientService
{
    Task<T> PostAsync<T>(string url, object paramObj, int timeout = 10, IContractResolver resolver = null);
}