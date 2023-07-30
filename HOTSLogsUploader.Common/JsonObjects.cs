using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace HOTSLogsUploader.Common;

public class UploadResponse
{
    public int Result { get; set; }
    public int? ReplayId { get; set; }
    public Guid Fingerprint { get; set; }
}

public class UpdateCheckResponse
{
    public UpdateCheckResponse(bool updateAvailable, string newVersion)
    {
        NewVersionAvailable = updateAvailable;
        NewVersion = newVersion;
    }

    public bool NewVersionAvailable { get; set; }
    public string NewVersion { get; set; }
}

public class FingerprintQuery
{
    public FingerprintQuery() { }

    public FingerprintQuery(IEnumerable<Guid> fingerprints)
        : this()
    {
        Fingerprints.AddRange(fingerprints);
    }


    public List<Guid> Fingerprints { get; } = new();


    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public StringContent ToStringContent()
    {
        return new StringContent(ToString(), Encoding.UTF8, "application/json");
    }
}

public class FingerprintResponse
{
    public List<(Guid Hash, bool IsDuplicate)> Fingerprints { get; } = new();


    public static FingerprintResponse Deserialize(string value)
    {
        return JsonConvert.DeserializeObject<FingerprintResponse>(value);
    }


    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
