using System;
using System.ServiceModel;
using System.Diagnostics;

namespace AdSage.Concert.Test.Framework
{
    public static class WCFHelper
    {
        public static void Using<T>(T client, Action<T> action)
            where T : ICommunicationObject
        {
            if (client == null)
            {
                throw new ArgumentNullException("WCF client object is null");
            }

            try
            {
                action(client);
            }
            finally
            {
                client.CloseConnection();
            }
        }

        /// <summary>
        /// Safely closes a service client connection.
        /// </summary>
        /// <param name="myServiceClient">The client connection to close.</param>
        public static void CloseConnection(this ICommunicationObject myServiceClient)
        {
            if (myServiceClient.State != CommunicationState.Opened)
            {
                return;
            }

            try
            {
                myServiceClient.Close();
            }
            catch (CommunicationException ex)
            {
                Debug.Print(ex.ToString());
                myServiceClient.Abort();
            }
            catch (TimeoutException ex)
            {
                Debug.Print(ex.ToString());
                myServiceClient.Abort();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                myServiceClient.Abort();
                throw;
            }
        }
    }
}
