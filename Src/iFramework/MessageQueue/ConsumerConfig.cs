﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFramework.MessageQueue
{
    public class ConsumerConfig
    {
        public static readonly ConsumerConfig DefaultConfig = new ConsumerConfig();
        public int BackOffIncrement { get; set; } = 30;
        public int FullLoadThreshold { get; set; } = 1000;
        public int WaitInterval { get; set; } = 1000;
        public int MailboxProcessBatchCount { get; set; } = 100;
        public string AutoOffsetReset { get; set; } = MessageQueue.AutoOffsetReset.Smallest;
        public Dictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();

        public object this[string key]
        {
            get => Extensions.TryGetValue(key, out var value) ? value : null;
            set => Extensions[key] = value;
        }
    }
}
