using SkimiaOS.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SkimiaOS.Server.BaseServer.Exceptions
{
    public class ExceptionManager : Singleton<ExceptionManager>
    {
        private List<Exception> m_exceptions = new List<Exception>();
        public ReadOnlyCollection<Exception> Exceptions
        {
            get
            {
                return this.m_exceptions.AsReadOnly();
            }
        }
        public void RegisterException(Exception ex)
        {
            this.m_exceptions.Add(ex);
        }
    }
}
