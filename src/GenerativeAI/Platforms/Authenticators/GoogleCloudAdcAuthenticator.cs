﻿using System.Diagnostics;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Logging;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Authenticators;

public class GoogleCloudAdcAuthenticator : BaseAuthenticator
{
    private ILogger? logger;
    private string? credentialFile;

    public GoogleCloudAdcAuthenticator(string? credentialFile = null, ILogger? logger = null)
    {
        this.credentialFile = credentialFile;
        this.logger = logger;
    }


    public override async Task<AuthTokens> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogAuthenticationStarted();

            var token = AcquireGcpAccessToken();

            var tokenInfo = await GetTokenInfo(token);

            logger?.LogAuthenticationEndedSuccessfully();
            return tokenInfo;
        }
        catch (Exception ex)
        {
            logger?.LogAuthenticationFailed(ex.Message);
            throw new AuthenticationException(
                $"Error while authenticating with Application Default Credentials.\r\n\r\n{ex.Message}");
        }
    }

    public override async Task<AuthTokens> RefreshAccessTokenAsync(AuthTokens token,
        CancellationToken cancellationToken = default)
    {
        return await GetAccessTokenAsync(cancellationToken);
    }

    /// <summary>
    /// Acquires an auth token for use with GCP services by invoking the gcloud CLI.
    /// This leverages Application Default Credentials to get the token from Google's environment.
    /// Reference: https://cloud.google.com/docs/authentication
    /// </summary>
    /// <returns>A string containing the access token.</returns>
    private string AcquireGcpAccessToken()
    {
        // Detect if running on Windows; adjust command accordingly.
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return ExecuteProcess(
                "cmd.exe",
                "/c gcloud auth application-default print-access-token"
            ).TrimEnd();
        }
        else
        {
            return ExecuteProcess(
                "gcloud",
                "auth application-default print-access-token"
            ).TrimEnd();
        }
    }

    /// <summary>
    /// Executes an external process, capturing its standard output and error output.
    /// Throws an exception if the process returns a non-zero exit code.
    /// </summary>
    /// <param name="tool">Name or path of the command to run.</param>
    /// <param name="arguments">Arguments to pass to the command, if any.</param>
    /// <returns>The standard output of the invoked process.</returns>
    private string ExecuteProcess(string tool, string arguments)
    {
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using (var proc = new Process())
        {
            proc.StartInfo.FileName = tool;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            // Capture stdout/stderr
            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };
            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) errorBuilder.AppendLine(e.Data);
            };

            try
            {
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                // Log as needed
                // e.g. Logger.LogRunExternalExe("Execution failed: " + ex.Message);
                return string.Empty;
            }

            // Check exit code and handle error
            if (proc.ExitCode == 0)
            {
                return outputBuilder.ToString();
            }
            else
            {
                var message = new StringBuilder();
                if (errorBuilder.Length > 0)
                {
                    message.AppendLine("Error output:");
                    message.AppendLine(errorBuilder.ToString());
                }

                if (outputBuilder.Length > 0)
                {
                    message.AppendLine("Standard output:");
                    message.AppendLine(outputBuilder.ToString());
                }

                throw new Exception(
                    $"Process '{tool} {arguments}' exited with code {proc.ExitCode}: {message}"
                );
            }
        }
    }
}