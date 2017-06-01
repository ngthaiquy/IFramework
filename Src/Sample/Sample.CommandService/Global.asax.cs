﻿using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using IFramework.Command;
using IFramework.Config;
using IFramework.Infrastructure.Logging;
using IFramework.IoC;
using IFramework.Message;
using IFramework.MessageQueue;
using IFramework.MessageQueue.MSKafka.Config;
using Sample.Command;
using Sample.Persistence;

namespace Sample.CommandService
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : HttpApplication
    {
        private static ILogger _Logger;
        private static IMessagePublisher _MessagePublisher;
        private static ICommandBus _CommandBus;
        private static IMessageConsumer _CommandConsumer1;
        private static IMessageConsumer _CommandConsumer2;
        private static IMessageConsumer _CommandConsumer3;
        private static IMessageConsumer _DomainEventConsumer;
        private static IMessageConsumer _ApplicationEventConsumer;


        public static void Bootstrap()
        {
            try
            {
                Configuration.Instance
                             .UseLog4Net()
                             .MessageQueueUseMachineNameFormat()
                             .UseMessageQueue()
                             .UseMessageStore<SampleModelContext>()
                             .UseKafka("localhost:2181")
                             //.UseEQueue()
                             .UseCommandBus(Environment.MachineName, linerCommandManager: new LinearCommandManager())
                             .UseMessagePublisher("eventTopic");
                _Logger = IoCFactory.Resolve<ILoggerFactory>().Create(typeof(WebApiApplication).Name);


                _Logger.Debug($"App Started");

                #region EventPublisher init

                _MessagePublisher = MessageQueueFactory.GetMessagePublisher();
                _MessagePublisher.Start();

                #endregion

                #region event subscriber init

                _DomainEventConsumer = MessageQueueFactory.CreateEventSubscriber("DomainEvent", "DomainEventSubscriber",
                                                                                 Environment.MachineName, 100, "DomainEventSubscriber");
                _DomainEventConsumer.Start();

                #endregion

                #region application event subscriber init

                _ApplicationEventConsumer = MessageQueueFactory.CreateEventSubscriber("AppEvent", "AppEventSubscriber",
                                                                                      Environment.MachineName, 100, "ApplicationEventSubscriber");
                _ApplicationEventConsumer.Start();

                #endregion

                #region CommandBus init

                _CommandBus = MessageQueueFactory.GetCommandBus();
                _CommandBus.Start();

                #endregion

                #region Command Consuemrs init'

                var commandQueueName = "commandqueue";
                _CommandConsumer1 =
                    MessageQueueFactory.CreateCommandConsumer(commandQueueName, "0", 100, "CommandHandlers");
                _CommandConsumer1.Start();

                _CommandConsumer2 =
                    MessageQueueFactory.CreateCommandConsumer(commandQueueName, "1", 100, "CommandHandlers");
                _CommandConsumer2.Start();

                _CommandConsumer3 =
                    MessageQueueFactory.CreateCommandConsumer(commandQueueName, "2", 100, "CommandHandlers");
                _CommandConsumer3.Start();

                #endregion
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.GetBaseException().Message, ex);
            }
        }

        // ZeroMQ Application_Start
        protected void Application_Start()
        {
            Bootstrap();
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            try
            {
                var endSuccesss = Task.WaitAll(new[]
                {
                    Task.Run(() => _CommandConsumer1?.Stop()),
                    Task.Run(() => _CommandConsumer2?.Stop()),
                    Task.Run(() => _CommandConsumer3?.Stop()),
                    Task.Run(() => _DomainEventConsumer?.Stop()),
                    Task.Run(() => _ApplicationEventConsumer?.Stop()),
                    Task.Run(() => _CommandBus?.Stop()),
                    Task.Run(() => _MessagePublisher?.Stop())
                }, 10000);
                if (!endSuccesss)
                {
                    throw new Exception($"stop message queue client timeout!");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.GetBaseException().Message, ex);
            }
            finally
            {
                IoCFactory.Instance.CurrentContainer.Dispose();
            }
            _Logger.Debug($"App Ended");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError().GetBaseException(); //获取错误
            _Logger.Error(ex.Message, ex);
        }
    }
}