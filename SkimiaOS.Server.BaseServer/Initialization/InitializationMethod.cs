﻿using System.Reflection;

namespace SkimiaOS.Server.BaseServer.Initialization
{
    public class InitializationMethod
    {
        public InitializationAttribute Attribute
        {
            get;
            private set;
        }
        public MethodInfo Method
        {
            get;
            private set;
        }
        public object Caller
        {
            get;
            set;
        }
        public bool Initialized
        {
            get;
            set;
        }
        public InitializationMethod(InitializationAttribute attribute, MethodInfo method)
        {
            this.Attribute = attribute;
            this.Method = method;
        }
    }
}