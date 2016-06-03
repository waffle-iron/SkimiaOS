using System;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public class Parameter<T> : IParameter
	{
		IParameterDefinition IParameter.Definition
		{
			get
			{
				return this.Definition;
			}
		}
		public IParameterDefinition<T> Definition
		{
			get;
			private set;
		}
		object IParameter.Value
		{
			get
			{
				return this.Value;
			}
		}
		public T Value
		{
			get;
			private set;
		}
		public bool IsDefined
		{
			get;
			private set;
		}
		public Parameter(IParameterDefinition<T> definition)
		{
			this.Definition = definition;
		}
		public void SetValue(string str, TriggerBase trigger)
		{
			this.Value = (T)((object)this.Definition.ConvertString(str, trigger));
			this.IsDefined = true;
		}
		public void SetDefaultValue(TriggerBase trigger)
		{
			this.Value = (T)((object)this.Definition.ConvertString(string.Empty, trigger));
			this.IsDefined = false;
		}
	}
}
