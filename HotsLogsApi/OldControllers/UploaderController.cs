// Copyright (c) StarkillerLLC. All rights reserved.

using HelperCore;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.MigrationControllers;
using HOTSLogsUploader.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Heroes.ReplayParser.DataParser;

namespace HotsLogsApi.OldControllers;

[Route("[controller]/[action]")]
[Migration]
public class UploaderController : ControllerBase
{
    private readonly UploadHelper _uploadHelper;

    public UploaderController(UploadHelper uploadHelper)
    {
        _uploadHelper = uploadHelper;
    }

    [HttpPost]
    public async Task<object> AddReplay([FromQuery] string fileName, [FromQuery] string uploaderVersion)
    {
        var request = Request;

        var (parseResult, _, replayGuid) = await _uploadHelper.AddReplay(
            request.Body,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "<unknown ip address>",
            uploaderVersion);

        var addReplayResult = parseResult == ReplayParseResult.Exception
            ? new UploadResponse
            {
                Result = (int)ReplayParseResult.Exception,
            }
            : new UploadResponse
            {
                Result = (int)parseResult,
                ReplayId = DataHelper.GetReplayID(replayGuid.GetValueOrDefault()),
                Fingerprint = replayGuid.GetValueOrDefault(),
            };

        return addReplayResult;
    }

    [HttpPost]
    public object CheckFingerprints([FromBody] FingerprintQuery query)
    {
        var response = new FingerprintResponse();

        foreach (var fingerprint in query.Fingerprints)
        {
            response.Fingerprints.Add((fingerprint, DataHelper.GetReplayID(fingerprint).HasValue));
        }

        return response;
    }

    [HttpGet]
    public object GetReplayId([FromQuery] Guid fingerprint)
    {
        return new { ReplayId = DataHelper.GetReplayID(fingerprint) };
    }

    [HttpGet]
    public object UpdateCheck([FromQuery] string version)
    {
        try
        {
            // Get version of uploader on the server
            var fileVersion = UploaderVersionHelper.Version?.FileVersion ?? "0.0.0.0";

            var clientVersion = new Version(version);
            var serverVersion = new Version(fileVersion);

            return new UpdateCheckResponse(serverVersion > clientVersion, fileVersion);
        }
        catch (Exception)
        {
            throw new Exception(); // return a 500 error
        }
    }

    [HttpPost]
    public async Task<object> Upload([FromQuery] string uploaderVersion)
    {
        var request = Request;
        var response = new List<UploadResponse>();

        foreach (var t in request.Form.Files)
        {
            try
            {
                await using var readStream = t.OpenReadStream();

                var (parseResult, _, replayGuid) = await _uploadHelper.AddReplay(
                    readStream,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "<unknown ip address>",
                    uploaderVersion);

                var addReplayResult = parseResult == ReplayParseResult.Exception
                    ? new UploadResponse
                    {
                        Result = (int)ReplayParseResult.Exception,
                    }
                    : new UploadResponse
                    {
                        Result = (int)parseResult,
                        ReplayId = DataHelper.GetReplayID(replayGuid.GetValueOrDefault()),
                        Fingerprint = replayGuid.GetValueOrDefault(),
                    };

                response.Add(addReplayResult);
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        return response;
    }
}
