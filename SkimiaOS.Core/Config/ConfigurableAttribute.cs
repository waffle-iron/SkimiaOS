using System;


namespace SkimiaOS.Core.Config
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ConfigurableAttribute : Attribute
    {
        public bool DefinableRunning
        {
            get;
            set;
        }
        public int Priority
        {
            get;
            set;
        }

        public ConfigurableAttribute()
        {
            this.Priority = 1;
        }

        public ConfigurableAttribute(bool definableByConfig = false)
        {
            this.DefinableRunning = definableByConfig;
            this.Priority = 1;
        }
    }
}
