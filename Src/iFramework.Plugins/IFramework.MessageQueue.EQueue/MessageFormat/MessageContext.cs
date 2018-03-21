﻿using System;
using System.Collections.Generic;
using System.Text;
using IFramework.Infrastructure;
using IFramework.Message;
using IFramework.Message.Impl;
using Newtonsoft.Json.Linq;

namespace IFramework.MessageQueue.EQueue.MessageFormat
{
    public class MessageContext : IMessageContext
    {
        private object _Message;

        private SagaInfo _sagaInfo;

        public MessageContext(EQueueMessage equeueMessage, string broker, int partition, long offset)
        {
            EqueueMessage = equeueMessage;
            Offset = offset;
            Partition = partition;
            ToBeSentMessageContexts = new List<IMessageContext>();
            MessageOffset = new MessageOffset(broker, partition, offset);
        }

        public MessageContext(object message, string id = null)
        {
            EqueueMessage = new EQueueMessage();
            SentTime = DateTime.Now;
            Message = message;
            if (!string.IsNullOrEmpty(id))
            {
                MessageId = id;
            }
            else if (message is IMessage)
            {
                MessageId = ((IMessage) message).Id;
            }
            else
            {
                MessageId = ObjectId.GenerateNewId().ToString();
            }
            ToBeSentMessageContexts = new List<IMessageContext>();
            if (message is IMessage)
            {
                Topic = ((IMessage) message).GetTopic();
            }
        }


        public MessageContext(IMessage message, string key)
            : this(message)
        {
            Key = key;
        }

        public MessageContext(IMessage message, string replyToEndPoint, string key)
            : this(message, key)
        {
            ReplyToEndPoint = replyToEndPoint;
        }

        public EQueueMessage EqueueMessage { get; protected set; }
        public int Partition { get; protected set; }
        public List<IMessageContext> ToBeSentMessageContexts { get; protected set; }
        public long Offset { get; protected set; }

        public IDictionary<string, object> Headers => EqueueMessage.Headers;

        public string Key
        {
            get => (string) Headers.TryGetValue("Key");
            set => Headers["Key"] = value;
        }

        public string CorrelationId
        {
            get => (string) Headers.TryGetValue("CorrelationID");
            set => Headers["CorrelationID"] = value;
        }

        public string MessageId
        {
            get => (string) Headers.TryGetValue("MessageID");
            set => Headers["MessageID"] = value;
        }

        public string ReplyToEndPoint
        {
            get => (string) Headers.TryGetValue("ReplyToEndPoint");
            set => Headers["ReplyToEndPoint"] = value;
        }

        public object Reply { get; set; }

        public object Message
        {
            get
            {
                if (_Message != null)
                {
                    return _Message;
                }

                if (Headers.TryGetValue("MessageType", out var messageType) && messageType != null)
                {
                    var jsonValue = Encoding.UTF8.GetString(EqueueMessage.Payload);
                    _Message = jsonValue.ToJsonObject(Type.GetType(messageType.ToString()));
                }
                return _Message;
            }
            protected set
            {
                _Message = value;
                EqueueMessage.Payload = Encoding.UTF8.GetBytes(value.ToJson());
                if (value != null)
                {
                    Headers["MessageType"] = value.GetType().AssemblyQualifiedName;
                }
            }
        }

        public DateTime SentTime
        {
            get => (DateTime) Headers.TryGetValue("SentTime");
            set => Headers["SentTime"] = value;
        }

        public string Topic
        {
            get => (string) Headers.TryGetValue("Topic");
            set => Headers["Topic"] = value;
        }

        public SagaInfo SagaInfo
        {
            get
            {
                if (_sagaInfo == null)
                {
                    var sagaInfoJson = Headers.TryGetValue("SagaInfo") as JObject;
                    if (sagaInfoJson != null)
                    {
                        try
                        {
                            _sagaInfo = ((JObject) Headers.TryGetValue("SagaInfo")).ToObject<SagaInfo>();
                        }
                        catch (Exception) { }
                    }
                }
                return _sagaInfo;
            }
            set => Headers["SagaInfo"] = _sagaInfo = value;
        }

        public string Ip
        {
            get => (string) Headers.TryGetValue("IP");
            set => Headers["IP"] = value;
        }

        public string Producer
        {
            get => (string) Headers.TryGetValue("Producer");
            set => Headers["Producer"] = value;
        }

        public MessageOffset MessageOffset { get; }
    }
}