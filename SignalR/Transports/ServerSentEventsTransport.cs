﻿using System;
using System.Web;
using System.Threading.Tasks;
using SignalR.Infrastructure;

namespace SignalR.Transports
{
    public class ServerSentEventsTransport : ForeverTransport
    {
        public ServerSentEventsTransport(HttpContextBase context, IJsonSerializer jsonSerializer)
            : base(context, jsonSerializer)
        {

        }

        protected override Task InitializeResponse(IConnection connection)
        {
            long lastMessageId;
            if (long.TryParse(Context.Request.Headers["Last-Event-ID"], out lastMessageId))
            {
                LastMessageId = lastMessageId;
            }

            return base.InitializeResponse(connection)
                .Success(_ =>
                {
                    Context.Response.ContentType = "text/event-stream";
                    return Context.Response.WriteAsync("data: initialized\n\n");
                }).FastUnwrap();
        }

        protected override bool IsConnectRequest
        {
            get
            {
                return Context.Request.Headers["Last-Event-ID"] == null;
            }
        }

        public override Task Send(PersistentResponse response)
        {
            if (Context.Response.IsClientConnected)
            {
                return Context.Response.WriteAsync("id: " + response.MessageId + "\n" + "data: " + JsonSerializer.Stringify(response) + "\n\n");
            }
            return TaskAsyncHelper.Empty;
        }
    }
}