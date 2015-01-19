// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.Linq;
using System.Net;
using Realtime.Http;
using Realtime.Tasks;

namespace Realtime.Messaging.Ortc
{
    /// <summary>
    /// Http Client for messaging
    /// </summary>
    public class MessageClient
    {
        private static HttpServiceClient _client;

        private static HttpServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpServiceClient();

                    _client.ContentType = "application/x-www-form-urlencoded";
                }

                return _client;
            }
        }
        
        /// <summary>
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="url">ORTC server URL.</param>
        /// <param name="isCluster">Indicates whether the ORTC server is in a cluster.</param>
        /// <param name="authenticationToken">Authentication Token which is generated by the application server, for instance a unique session ID.</param>
        /// <param name="applicationKey">Application Key that was provided to you together with the ORTC service purchasing.</param>
        /// <param name="privateKey">The private key provided to you together with the ORTC service purchasing.</param>
        /// <param name="channel">The channel where the message will be sent.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the send was successful or false if it was not.</returns>
        public static Task<bool> SendMessageAsync(string url, bool isCluster, string authenticationToken, string applicationKey, string privateKey, string channel, string message)
        {
            return new Task<bool>(SendMessage(url, isCluster, authenticationToken, applicationKey, privateKey, channel, message));
        }

        /// <summary>
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="url">ORTC server URL.</param>
        /// <param name="isCluster">Indicates whether the ORTC server is in a cluster.</param>
        /// <param name="authenticationToken">Authentication Token which is generated by the application server, for instance a unique session ID.</param>
        /// <param name="applicationKey">Application Key that was provided to you together with the ORTC service purchasing.</param>
        /// <param name="privateKey">The private key provided to you together with the ORTC service purchasing.</param>
        /// <param name="channel">The channel where the message will be sent.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the send was successful or false if it was not.</returns>
        public static bool SendMessage(string url, bool isCluster, string authenticationToken, string applicationKey, string privateKey, string channel, string message)
        {
            string connectionUrl = url;

            if (String.IsNullOrEmpty(url))
            {
                throw new OrtcException(OrtcExceptionReason.InvalidArguments, "Server URL can not be null or empty.");
            }

            if (isCluster)
            {
                connectionUrl = ClusterClient.GetClusterServerWithRetry(url, applicationKey);
            }

            connectionUrl = connectionUrl.Last() == '/' ? connectionUrl : connectionUrl + "/";

            var postParameters = String.Format("AT={0}&AK={1}&PK={2}&C={3}&M={4}", authenticationToken, applicationKey, privateKey, channel, HttpUtility.UrlEncode(message));


            var hTask = Client.PostAsync(String.Format("{0}send", connectionUrl), postParameters);

            hTask.Wait();

            if (hTask.IsFaulted)
                throw hTask.Exception;

            return hTask.StatusCode == HttpStatusCode.Created;
        }
    }
}