﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IFramework.Infrastructure.Mailboxes.Impl
{
    public class MailboxMessage
    {
        public Func<Task> Task { get; set; }
        public string Key { get; set; }
        public TaskCompletionSource<object> TaskCompletionSource { get;set; }

        public MailboxMessage(string key, Func<Task> task, TaskCompletionSource<object> taskCompletionSource = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            Key = key;
            Task = task;
            TaskCompletionSource = taskCompletionSource ?? new TaskCompletionSource<object>();
        }
    }
}
