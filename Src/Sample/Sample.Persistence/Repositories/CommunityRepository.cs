﻿using System.ComponentModel;
using IFramework.DependencyInjection;
using IFramework.Repositories;
using IFramework.UnitOfWork;
using Sample.Domain;

namespace Sample.Persistence.Repositories
{
    public class CommunityRepository : DomainRepository, ICommunityRepository
    {
        public CommunityRepository(IObjectProvider objectProvider)
            : base(objectProvider) { }
    }
}