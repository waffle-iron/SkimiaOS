using NLog;
using NLog.Config;
using NLog.Internal;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Core.Log
{

    /// <summary>
    /// The call site (class name, method name and source information).
    /// </summary>
    [LayoutRenderer("custom-callsite")]
    [ThreadAgnostic]
    public class CustomCallSiteLayoutRenderer : LayoutRenderer, IUsesStackTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallSiteLayoutRenderer" /> class.
        /// </summary>
        public CustomCallSiteLayoutRenderer()
        {
            this.ClassName = true;
            this.MethodName = true;
            this.CleanNamesOfAnonymousDelegates = false;
#if !SILVERLIGHT
            this.FileName = false;
            this.IncludeSourcePath = true;
#endif
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the class name.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool ClassName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the method name.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool MethodName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the class namespace.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool Namespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the class namespace.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("no")]
        public string Opt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the class namespace.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(Int32.MaxValue)]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the method name will be cleaned up if it is detected as an anonymous delegate.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool CleanNamesOfAnonymousDelegates { get; set; }

        /// <summary>
        /// Gets or sets the number of frames to skip.
        /// </summary>
        [DefaultValue(0)]
        public int SkipFrames { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets a value indicating whether to render the source file name and line number.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source file path.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool IncludeSourcePath { get; set; }
#endif

        /// <summary>
        /// Gets the level of stack trace information required by the implementing class.
        /// </summary>
        StackTraceUsage IUsesStackTrace.StackTraceUsage
        {
            get
            {
#if !SILVERLIGHT
                if (this.FileName)
                {
                    return StackTraceUsage.Max;
                }
#endif

                return StackTraceUsage.WithoutSource;
            }
        }

        /// <summary>
        /// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder dbuilder, LogEventInfo logEvent)
        {
            StringBuilder _builder = new StringBuilder();
            StackFrame frame = logEvent.StackTrace != null ? logEvent.StackTrace.GetFrame(logEvent.UserStackFrameNumber + SkipFrames) : null;
            if (frame != null)
            {
                MethodBase method = frame.GetMethod();
                if (this.ClassName)
                {
                    if (method.DeclaringType != null)
                    {
                        string className = method.DeclaringType.FullName;

                        if (!this.Namespace)
                        {
                            if (className.Contains(method.DeclaringType.Namespace))
                            {
                                className = className.Substring(method.DeclaringType.Namespace.Length+1);
                            }
                        }
                        if (this.CleanNamesOfAnonymousDelegates)
                        {
                            // NLog.UnitTests.LayoutRenderers.CallSiteTests+<>c__DisplayClassa
                            if (className.Contains("+<>"))
                            {
                                int index = className.IndexOf("+<>");
                                className = className.Substring(0, index);
                            }
                        }

                        _builder.Append(className);
                    }
                    else
                    {
                        _builder.Append("<no type>");
                    }
                }

                if (this.MethodName)
                {
                    if (this.ClassName)
                    {
                        _builder.Append(".");
                    }

                    if (method != null)
                    {
                        string methodName = method.Name;

                        if (this.CleanNamesOfAnonymousDelegates)
                        {
                            // Clean up the function name if it is an anonymous delegate
                            // <.ctor>b__0
                            // <Main>b__2
                            if (methodName.Contains("__") == true && methodName.StartsWith("<") == true && methodName.Contains(">") == true)
                            {
                                int startIndex = methodName.IndexOf('<') + 1;
                                int endIndex = methodName.IndexOf('>');

                                methodName = methodName.Substring(startIndex, endIndex - startIndex);
                            }
                        }

                        _builder.Append(methodName);
                    }
                    else
                    {
                        _builder.Append("<no method>");
                    }
                }

#if !SILVERLIGHT
                if (this.FileName)
                {
                    string fileName = frame.GetFileName();
                    if (fileName != null)
                    {
                        _builder.Append("(");
                        if (this.IncludeSourcePath)
                        {
                            _builder.Append(fileName);
                        }
                        else
                        {
                            _builder.Append(Path.GetFileName(fileName));
                        }

                        _builder.Append(":");
                        _builder.Append(frame.GetFileLineNumber());
                        _builder.Append(")");
                    }
                }
#endif
            }

            //test

            string str = _builder.ToString();
            if(this.Opt == "blockcenter")
            {
                string pad = "";
                int AddLeftRight = (this.Length - str.Length) / 2;
                pad = pad.PadLeft(AddLeftRight);
                str = pad + str + pad;
                if (str.Length < this.Length)
                    str += " ";
            }
            
            dbuilder.Append(str);

        }
    }
}
