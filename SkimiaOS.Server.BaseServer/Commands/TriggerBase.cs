using SkimiaOS.Core.IO;
using SkimiaOS.Server.BaseServer.Commands.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
namespace SkimiaOS.Server.BaseServer.Commands
{
	public abstract class TriggerBase
	{

        private readonly Regex m_regexIsNamed = new Regex("^(?!\\\")(?:-|--)?([\\w\\d]+)=(.*)$", RegexOptions.Compiled);
        private readonly Regex m_regexVar = new Regex("^(?!\\\")(?:-|--)([\\w\\d]+)(?!\\\")$", RegexOptions.Compiled);
        public abstract ICommandsUser User
        {
            get;
        }
        public StringStream Args
        {
            get;
            private set;
        }
        public virtual RoleEnum UserRole
        {
            get;
            private set;
        }
        public CommandBase BoundCommand
        {
            get;
            private set;
        }
        public abstract bool CanFormat
        {
            get;
        }
        internal Dictionary<string, IParameter> CommandsParametersByName
        {
            get;
            private set;
        }
        internal Dictionary<string, IParameter> CommandsParametersByShortName
        {
            get;
            private set;
        }
        private TriggerBase()
        {
            this.CommandsParametersByName = new Dictionary<string, IParameter>();
            this.CommandsParametersByShortName = new Dictionary<string, IParameter>();
        }
        protected TriggerBase(StringStream args, RoleEnum userRole)
            : this()
        {
            this.Args = args;
            this.UserRole = userRole;
        }
        protected TriggerBase(string args, RoleEnum userRole)
            : this(new StringStream(args), userRole)
        {
        }
        public virtual bool CanAccessCommand(CommandBase command)
        {
            return command.RequiredRole <= this.UserRole;
        }
        public abstract void Reply(string text);
        public void Reply(string format, params object[] args)
        {
            this.Reply(string.Format(format, args));
        }
        public void ReplyBold(string format, params object[] args)
        {
            this.Reply(string.Format(format, args.Select(new Func<object, string>(this.Bold)).ToArray<string>()));
        }
        public virtual void ReplyError(string message)
        {
            if (!this.CanFormat)
            {
                this.Reply("(Error) " + message);
            }
            else
            {
                this.Reply(this.Bold("(Error)") + " " + message);
            }
        }
        public void ReplyError(string format, params object[] args)
        {
            this.ReplyError(string.Format(format, args));
        }
        public string Bold(object obj)
        {
            return this.Bold(obj.ToString());
        }
        public string Bold(string message)
        {
            string result;
            if (!this.CanFormat)
            {
                result = message;
            }
            else
            {
                result = "<b>" + message + "</b>";
            }
            return result;
        }
        public string Underline(object obj)
        {
            return this.Underline(obj.ToString());
        }
        public string Underline(string message)
        {
            string result;
            if (!this.CanFormat)
            {
                result = message;
            }
            else
            {
                result = "<u>" + message + "</u>";
            }
            return result;
        }
        public string Italic(object obj)
        {
            return this.Italic(obj.ToString());
        }
        public string Italic(string message)
        {
            string result;
            if (!this.CanFormat)
            {
                result = message;
            }
            else
            {
                result = "<i>" + message + "</i>";
            }
            return result;
        }
        public string Color(object obj, Color color)
        {
            return this.Color(obj.ToString(), color);
        }
        public string Color(string message, Color color)
        {
            string result;
            if (!this.CanFormat)
            {
                result = message;
            }
            else
            {
                result = string.Concat(new string[]
				{
					"<font color=\"#",
					color.ToArgb().ToString("X"),
					"\">",
					message,
					"</font>"
				});
            }
            return result;
        }
        public virtual T Get<T>(string name)
        {
            T result;
            if (this.CommandsParametersByName.ContainsKey(name))
            {
                result = (T)((object)this.CommandsParametersByName[name].Value);
            }
            else
            {
                if (!this.CommandsParametersByShortName.ContainsKey(name))
                {
                    throw new ArgumentException("'" + name + "' is not an existing parameter");
                }
                result = (T)((object)this.CommandsParametersByShortName[name].Value);
            }
            return result;
        }
        public virtual bool IsArgumentDefined(string name)
        {
            bool result;
            if (this.CommandsParametersByName.ContainsKey(name))
            {
                result = this.CommandsParametersByName[name].IsDefined;
            }
            else
            {
                result = (this.CommandsParametersByShortName.ContainsKey(name) && this.CommandsParametersByShortName[name].IsDefined);
            }
            return result;
        }
        //public abstract BaseClient GetSource();
        public int RegisterException(Exception ex)
        {
            this.User.CommandsErrors.Add(new KeyValuePair<string, Exception>(this.Args.String, ex));
            return this.User.CommandsErrors.Count - 1;
        }
        public bool BindToCommand(CommandBase command)
        {
            this.BoundCommand = command;
            bool result;
            if (command is SubCommandContainer)
            {
                result = true;
            }
            else
            {
                List<IParameter> list = new List<IParameter>();
                List<IParameterDefinition> list2 = new List<IParameterDefinition>(this.BoundCommand.Parameters);
                if (list2.Count == 1 && list2[0].ValueType == typeof(string) && !list2[0].IsOptional)
                {
                    IParameter parameter = list2[0].CreateParameter();
                    parameter.SetValue(this.Args.NextWords(), this);
                    list.Add(parameter);
                    list2.Remove(list2[0]);
                }
                if (this.BoundCommand.Parameters.Count == 0)
                {
                    result = true;
                }
                else
                {
                    string text = this.Args.NextWord();
                    bool flag = false;
                    while (!string.IsNullOrEmpty(text) && list.Count < this.BoundCommand.Parameters.Count)
                    {
                        if (text.StartsWith("\"") && text.EndsWith("\""))
                        {
                            text = text.Remove(text.Length - 1, 1).Remove(0, 1);
                        }
                        bool flag2 = false;
                        if (text.StartsWith("-"))
                        {
                            string name = null;
                            string text2 = null;
                            Match match = this.m_regexIsNamed.Match(text);
                            if (match.Success)
                            {
                                name = match.Groups[1].Value;
                                text2 = match.Groups[2].Value;
                                if (text2.StartsWith("\"") && text2.EndsWith("\""))
                                {
                                    text2 = text2.Remove(text2.Length - 1, 1).Remove(0, 1);
                                }
                            }
                            else
                            {
                                Match match2 = this.m_regexVar.Match(text);
                                if (match2.Success)
                                {
                                    name = match2.Groups[1].Value;
                                    text2 = string.Empty;
                                    flag = true;
                                }
                            }
                            if (!string.IsNullOrEmpty(name))
                            {
                                IParameterDefinition parameterDefinition = list2.SingleOrDefault((IParameterDefinition entry) => TriggerBase.CompareParameterName(entry, name, CommandBase.IgnoreCommandCase));
                                if (parameterDefinition != null)
                                {
                                    IParameter parameter2 = parameterDefinition.CreateParameter();
                                    try
                                    {
                                        if (flag && parameterDefinition.ValueType == typeof(bool))
                                        {
                                            text2 = "true";
                                        }
                                        parameter2.SetValue(text2, this);
                                    }
                                    catch (ConverterException ex)
                                    {
                                        this.ReplyError(ex.Message);
                                        result = false;
                                        return result;
                                    }
                                    catch (Exception ex2)
                                    {
                                        this.ReplyError("Cannot parse : {0} as {1} (error-index:{2})", new object[]
										{
											text,
											parameterDefinition.ValueType,
											this.RegisterException(ex2)
										});
                                        result = false;
                                        return result;
                                    }
                                    list.Add(parameter2);
                                    list2.Remove(parameterDefinition);
                                    flag2 = true;
                                }
                            }
                        }
                        if (!flag2)
                        {
                            IParameterDefinition parameterDefinition = list2.First<IParameterDefinition>();
                            IParameter parameter2 = parameterDefinition.CreateParameter();
                            try
                            {
                                parameter2.SetValue(text, this);
                            }
                            catch (ConverterException ex)
                            {
                                this.ReplyError(ex.Message);
                                result = false;
                                return result;
                            }
                            catch (Exception ex2)
                            {
                                this.ReplyError("Cannot parse : {0} as {1} (error-index:{2})", new object[]
								{
									text,
									parameterDefinition.ValueType,
									this.RegisterException(ex2)
								});
                                result = false;
                                return result;
                            }
                            list.Add(parameter2);
                            list2.Remove(parameterDefinition);
                        }
                        text = this.Args.NextWord();
                    }
                    foreach (IParameterDefinition current in list2)
                    {
                        if (!current.IsOptional)
                        {
                            this.ReplyError("{0} is not defined", new object[]
							{
								current.Name
							});
                            result = false;
                            return result;
                        }
                        IParameter parameter2 = current.CreateParameter();
                        parameter2.SetDefaultValue(this);
                        list.Add(parameter2);
                    }
                    this.CommandsParametersByName = list.ToDictionary((IParameter entry) => entry.Definition.Name);
                    this.CommandsParametersByShortName = list.ToDictionary((IParameter entry) => (!string.IsNullOrEmpty(entry.Definition.ShortName)) ? entry.Definition.ShortName : entry.Definition.Name);
                    result = true;
                }
            }
            return result;
        }
        public static bool CompareParameterName(IParameterDefinition parameter, string name, bool useCase)
        {
            return name.Equals(parameter.Name, useCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) || name.Equals(parameter.ShortName, useCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }
    }
}
