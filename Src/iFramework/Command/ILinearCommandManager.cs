﻿using System;

namespace IFramework.Command
{
    public interface ILinearCommandManager
    {
        object GetLinearKey(ILinearCommand command);

        void RegisterLinearCommand<TLinearCommand>(Func<TLinearCommand, object> func)
            where TLinearCommand : ILinearCommand;
    }
}