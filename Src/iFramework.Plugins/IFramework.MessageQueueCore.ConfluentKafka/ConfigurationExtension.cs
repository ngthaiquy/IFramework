﻿using IFramework.Config;
using IFramework.DependencyInjection;
using IFramework.MessageQueue;
using Microsoft.Extensions.DependencyInjection;

namespace IFramework.MessageQueueCore.ConfluentKafka
{
    public static class ConfigurationExtension
    {
        public static Configuration UseConfluentKafka(this Configuration configuration,
                                                      string brokerList)
        {
            IoCFactory.Instance
                      .ObjectProviderBuilder
                      .Register<IMessageQueueClient, ConfluentKafkaClient>(ServiceLifetime.Singleton,
                                                                               new ConstructInjection(new ParameterInjection("brokerList", brokerList)));
            return configuration;
        }
    }
}