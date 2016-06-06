using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Core.Messages
{
    public class MessageHandlerAttribute : Attribute
    {
        public MessageHandlerAttribute()
        {
            HandleChildMessages = true;
        }

        public MessageHandlerAttribute(Type type)
        {
            MessageType = type;
            HandleChildMessages = true;
        }

        public Type MessageType
        {
            get;
            set;
        }

        public ListenerEntry FromFilter
        {
            get;
            set;
        }

        public ListenerEntry DestinationFilter
        {
            get;
            set;
        }

        public Type FilterType
        {
            get;
            set;
        }

        public bool HandleChildMessages
        {
            get;
            set;
        }
    }
}
