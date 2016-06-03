using System;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public interface IParameterDefinition
	{
		string Name
		{
			get;
		}
		string ShortName
		{
			get;
		}
		string Description
		{
			get;
		}
		bool IsOptional
		{
			get;
		}
		Type ValueType
		{
			get;
		}
		object ConvertString(string value, TriggerBase trigger);
		IParameter CreateParameter();
		string GetUsage();
	}
	public interface IParameterDefinition<out T> : IParameterDefinition
	{
		ConverterHandler<T> Converter
		{
			get;
		}
		T DefaultValue
		{
			get;
		}
	}
}
