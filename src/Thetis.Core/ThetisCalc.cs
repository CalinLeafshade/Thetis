using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thetis.Plugin;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using System.Data;

namespace Thetis.Core
{
    public class ThetisCalc : IThetisPlugin
    {

        private bool isExpressionAllowed(string expression)
        {
            string allowed = "+-/* 1234567890().";
            for (int i = 0; i < expression.Length; i++)
            {
                if (!allowed.Contains(expression.Substring(i, 1))) return false;
            }
            return true;
        }

        private double Evaluate(string expression)
        {

            if (!isExpressionAllowed(expression))
            {
                throw new InvalidExpressionException();
            }

            string source = @"
			class MyMath
			{
			    public static double Evaluate()
			    {
			        return <!expression!>;
			    }
			}
			";

            string finalSource = source.Replace("<!expression!>", expression);

            
            CodeSnippetCompileUnit compileUnit = new CodeSnippetCompileUnit(finalSource);
            CodeDomProvider provider = new CSharpCodeProvider();

            CompilerParameters parameters = new CompilerParameters();

            CompilerResults results = provider.CompileAssemblyFromDom(parameters, compileUnit);

            Type type = results.CompiledAssembly.GetType("MyMath");
            MethodInfo method = type.GetMethod("Evaluate");

            // The first parameter is the instance to invoke the method on. Because our Evaluate method is static, we pass null.
            double result = (double)method.Invoke(null, null);
            return result;
            
            
        }

        private IThetisPluginHost host;

        public IThetisPluginHost Host
        {
            set { host = value; }
        }

        public string Name
        {
            get { return "Calc"; }
        }

        public int Priority
        {
            get { return 0; }
        }

        public bool IgnoreClaimed
        {
            get { return true; }
        }

        public PluginResponse ChannelMessageReceived(MessageData message)
        {
            PluginResponse pr = new PluginResponse();
            if (message.Direct && message.LowerCaseMessage.StartsWith("calc"))
            {
                pr.Claimed = true;
                if (message.Message.Length > 5)
                {
                    string exp = message.LowerCaseMessage.Substring(5);
                    if (!isExpressionAllowed(exp))
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, "Expression contains disallowed characters");
                    }
                    else
                    {
                        double answer = 0;
                        try
                        {
                            answer = Evaluate(exp);
                            host.SendToChannel(MessageType.Message, message.Channel, String.Format("{0}, the answer is {1}", message.SentFrom.Nick, answer));
                        }
                        catch (Exception ex)
                        {
                            host.SendToChannel(MessageType.Message, message.Channel, "Expression compilation failed. You were almost certainly doing something silly");
                        }
                        
                    }
                }
                else
                {
                    host.SendToChannel(MessageType.Message, message.Channel, "Calc what? Be more constructive with your feedback, please.");
                }
            }
            return pr;
        }

        public void Closing()
        {
            
        }

        public void Tick()
        {
            
        }

        public void Init()
        {
            host.RegisterCommand(this, "calc");
        }

        public bool ForceSave()
        {
            return true;
        }

        public string GetHelp(string command)
        {
            if (command == "calc")
            {
                return "For calculating mathematical expressions. Usage: calc 3 + 5 * 75";
            }
            return null;
        }
    }
}
