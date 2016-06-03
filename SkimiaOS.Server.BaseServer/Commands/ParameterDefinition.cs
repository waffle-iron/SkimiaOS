using SkimiaOS.Core.Reflection;
using System;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public class ParameterDefinition<T> : IParameterDefinition<T>, IParameterDefinition
	{
		public ConverterHandler<T> Converter
		{
			get;
			private set;
		}
		public string Name
		{
			get;
			private set;
		}
		public string ShortName
		{
			get;
			private set;
		}
		public string Description
		{
			get;
			private set;
		}
		public bool IsOptional
		{
			get;
			private set;
		}
		public T DefaultValue
		{
			get;
			private set;
		}
		public Type ValueType
		{
			get
			{
				return typeof(T);
			}
		}
		public ParameterDefinition(string name, string shortName = "", string description = "", T defaultValue = default(T), bool isOptional = false, ConverterHandler<T> converter = null)
		{
			this.Name = name;
			this.ShortName = shortName;
			this.Description = description;
			this.DefaultValue = defaultValue;
			this.IsOptional = isOptional;
			this.Converter = converter;
			if (!object.Equals(defaultValue, default(T)))
			{
				this.IsOptional = true;
			}
		}
		public object ConvertString(string value, TriggerBase trigger)
		{
			object result;
			if (string.IsNullOrEmpty(value))
			{
				result = this.DefaultValue;
			}
			else
			{
				if (this.Converter != null && trigger != null)
				{
					result = this.Converter(value, trigger);
				}
				else
				{
					if (this.ValueType == typeof(string))
					{
						result = value;
					}
					else
					{
						if (this.ValueType.HasInterface(typeof(IConvertible)))
						{
							result = Convert.ChangeType(value, typeof(T));
						}
						else
						{
							result = (this.ValueType.IsEnum ? Enum.Parse(this.ValueType, value) : this.DefaultValue);
						}
					}
				}
			}
			return result;
		}
		public IParameter CreateParameter()
		{
			return new Parameter<T>(this);
		}
		public string GetUsage()
		{
			string text = (this.Name != this.ShortName) ? (this.Name + "/" + this.ShortName) : this.Name;
			if (!object.Equals(this.DefaultValue, default(T)))
			{
				text = text + "=" + this.DefaultValue;
			}
			return (!this.IsOptional) ? text : ("[" + text + "]");
		}
		public override string ToString()
		{
			return this.GetUsage();
		}
	}
}
