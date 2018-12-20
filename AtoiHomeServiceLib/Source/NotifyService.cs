using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace AtoiHomeServiceLib
{
    public class ClientInfo
    {
        public string UserId { get; set; }
        public ICallbackService Callback;
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class NotifyService : INotifyService
    {
        // create TextTransferEvent instance
        public event TextTransferEvent onTextTransferEvent = delegate { };
        // Notifycation 서비스에 접속한 클라이언트 목록
        public static List<ClientInfo> Clients { get; set; } = new List<ClientInfo>();

        public void Connect(TextTransferEventArgs e)
        {
            ICallbackService Callback = OperationContext.Current.GetCallbackChannel<ICallbackService>();
            ClientInfo clientInfo = new ClientInfo();
            clientInfo.UserId = e.UserId;
            clientInfo.Callback = Callback;

            // 이전 연결됐던 클라이언트가 비정상종료되고 다시 연결하려고 할때를 고려해서
            // 동일한 클라이언트 정보가 있다면 삭제하고 재등록한다.
            // 클라이언트 연결이 끊어졌을때 바로 처리하는 방법도 추가해야됨
            ClientInfo Client = Clients.Find(x => x.UserId.Equals(e.UserId));
            if (Client != null)
                Clients.Remove(Client);
            // 위에서 동일한 클라이언트 정보의 유무를 확인했기 때문에 재검증이 필요없을 것으로 보임. 조건문 삭제예정
            if (!Clients.Contains(clientInfo))
            {
                Clients.Add(clientInfo);
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.CONNECTED_CLIENT, "I'm a atoi", null));
            }
        }

        /// <summary>
        /// 클라이언트가 정상적으로 파이프를 끊기 위해서 호출하는 오퍼레이션
        /// </summary>
        /// <param name="e"></param>
        public void Disconnect(TextTransferEventArgs e)
        {
            ClientInfo Client = Clients.Find(x => x.UserId.Equals(e.UserId));
            if (Client != null)
            {
                Clients.Remove(Client);
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.DISCONNECTED_CLIENT, "I'm a atoi, bye!!!", null));
            }
        }

        /// <summary>
        /// NotifyService에 연결된 모든 클라이언트에게 메시지 브로드캐스팅
        /// </summary>
        /// <param name="action"></param> 잘 모르겠음
        void CallbackAllClients(Action<ICallbackService> action)
        {
            //Program.log.DebugFormat("Invoked CallbackAllClients");
            for (int i = Clients.Count - 1; i >= 0; i--)
            {
                ICallbackService callback = Clients[i].Callback;
                if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                {
                    try
                    {
                        // callback.SendCallbackMessage를 실행하는 것인데 문법이 낯설어서 :(
                        action(callback);
                    }
                    catch (Exception e)
                    {
                        Clients.RemoveAt(i);
                    }
                }
                else
                {
                    Clients.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Notify 서비스에서 클라이언트들에게 메시지를 전송하는 메서드
        /// </summary>
        /// <param name="e"></param>
        public void SendMessage(TextTransferEventArgs e)
        {
            if (e.MessageType == MessageType.NOTIFYSERVICE_CLOSING)
                // 서비스 중지하기전에 클라이언트들에게 메시지 브로드캐스팅
                CallbackAllClients(client => client.SendCallbackMessage(e));
            else
            {
                // TextTransfer 서비스에서 전달된 정보로 Notify 서비스에 연결된 클라이언트 중 
                // 사용자 정보가 일치하는 클라이언트에게 메시지 전송
                ClientInfo Client = Clients.Find(x => x.UserId.Equals(e.UserId));
                try
                {
                    if (Client != null)
                    {
                        Client.Callback.SendCallbackMessage(e);
                    }
                }
                catch (Exception ex)
                {
                    Clients.Remove(Client);
                }
            }
        }
    }
}
