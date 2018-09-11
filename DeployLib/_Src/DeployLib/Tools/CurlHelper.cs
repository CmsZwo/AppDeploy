using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeployLib
{
	public interface ICurlHelper
	{
		Task<HttpStatusCode> Curl(string url);
	}

	public class CurlHelper : ICurlHelper
	{
		public async Task<HttpStatusCode> Curl(string url)
		{
			using (var client = new HttpClient())
			{
				try
				{
					var result = await client.GetStringAsync(url);
					return HttpStatusCode.OK;
				}
				catch (HttpRequestException)
				{
					return HttpStatusCode.InternalServerError;
				}
			}
		}
	}
}
