﻿using System;

namespace CQRSCore.Config
{
    public interface IServiceLocator 
	{
        T GetService<T>();
        object GetService(Type type);
    }
}