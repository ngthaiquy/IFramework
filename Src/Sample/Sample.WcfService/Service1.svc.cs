﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using IFramework.Infrastructure.Logging;
using IFramework.IoC;
using IFramework.UnitOfWork;
using Sample.CommandHandler.Community;
using Sample.Domain;
using Sample.Domain.Model;

namespace Sample.WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        private ILogger _logger;
        private ICommunityRepository _repository;
        private IUnitOfWork _unitOfWork;
        public Service1(ICommunityRepository repository, IAppUnitOfWork unitOfWork, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(this.GetType().Name, additionalProperties: new { Additional1 = "additional1" });
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public string GetData(int value)
        {
            return $"You entered: {value}";
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException(nameof(composite));
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            var accounts = _repository.FindAll<Account>().ToList();
            composite.StringValue = accounts.Count.ToString();
            _logger.Debug($"repository find account {accounts.Count}");
            _logger.Debug(composite);
            return composite;
        }
    }
}
