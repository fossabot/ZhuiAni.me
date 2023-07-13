using System.Runtime.CompilerServices;
using Me.Xfox.ZhuiAnime.External.Bangumi.Models;
using RestSharp;

namespace Me.Xfox.ZhuiAnime.External.Bangumi;

public partial class BangumiApi
{
    protected const string DEFAULT_BANGUMI_API_HOST = "https://api.bgm.tv/";
    protected const string DEFUALT_USER_AGENT = "xfoxfu/zhuianime (https://github.com/xfoxfu/ZhuiAni.me)";

    private RestClient Client { get; init; }
    public string UserAgent => Client.Options.UserAgent!;

    public BangumiApi(
        string baseUrl = DEFAULT_BANGUMI_API_HOST,
        string userAgent = DEFUALT_USER_AGENT)
    {
        Client = new(new RestClientOptions(baseUrl)
        {
            UserAgent = userAgent,
        });
    }

    #region /v0/subjects
    public async Task<Subject> GetSubjectAsync(int id, CancellationToken ct = default)
    {
        var request = new RestRequest("/v0/subjects/{subject_id}", Method.Get)
            .AddUrlSegment("subject_id", id);
        return await GetResponseAsync<Subject>(request, ct);
    }
    #endregion

    #region /v0/episodes
    public async IAsyncEnumerable<Episode> GetEpisodesAsync(
        int subjectId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        uint offset = 0;
        uint total;
        do
        {
            if (ct.IsCancellationRequested) yield break;
            var request = new RestRequest("/v0/episodes", Method.Get)
                .AddQueryParameter("subject_id", subjectId)
                .AddQueryParameter("offset", offset);
            var response = await GetResponseAsync<PaginatedResult<Episode>>(request, ct);
            foreach (var episode in response.Data)
            {
                yield return episode;
            }
            offset = response.Offset + response.Limit;
            total = response.Total;
            Console.WriteLine($"O={offset} T={total}");
        } while (offset < total);
    }
    #endregion

    #region utils
    public async Task<T> GetResponseAsync<T>(RestRequest request, CancellationToken ct = default)
    {
        var response = await Client.ExecuteAsync<T>(request, ct);
        if (response.ErrorException != null) throw BangumiException.FromResponse(response);

        System.Diagnostics.Debug.Assert(response.Data != null);
        return response.Data;
    }

    public async Task<byte[]> GetBytesAsync(RestRequest request, CancellationToken ct = default)
    {
        var response = await Client.GetAsync(request, ct);
        return response.RawBytes!;
    }

    public async Task<byte[]> GetBytesAsync(string url, CancellationToken ct = default)
        => await GetBytesAsync(new RestRequest(url), ct);
    #endregion
}
