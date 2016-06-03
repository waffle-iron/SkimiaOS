using SkimiaOS.Core.Config;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SkimiaOS.Server.BaseServer.Commands
{
    public abstract class CommandBase
    {
        [Configurable]
        public static bool IgnoreCommandCase = true;
        public string[] Aliases
        {
            get;
            protected set;
        }
        public string Usage
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public RoleEnum RequiredRole
        {
            get;
            set;
        }
        public List<IParameterDefinition> Parameters
        {
            get;
            protected set;
        }
        protected CommandBase()
        {
            this.Parameters = new List<IParameterDefinition>();
        }
        public void AddParameter<T>(string name, string shortName = "", string description = "", T defaultValue = default(T), bool isOptional = false, ConverterHandler<T> converter = null)
        {
            this.Parameters.Add(new ParameterDefinition<T>(name, shortName, description, defaultValue, isOptional, converter));
        }
        public string GetSafeUsage()
        {
            string result;
            if (!string.IsNullOrEmpty(this.Usage))
            {
                result = this.Usage;
            }
            else
            {
                if (this.Parameters == null)
                {
                    result = "";
                }
                else
                {
                    result = string.Join(" ",
                        from entry in this.Parameters
                        select entry.GetUsage());
                }
            }
            return result;
        }
        public abstract void Execute(TriggerBase trigger);
        public override string ToString()
        {
            return base.GetType().Name;
        }
    }
}
