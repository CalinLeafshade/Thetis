using System;
using Thetis.Plugin;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Thetis.Games
{
	
	public class CountdownRound
	{
		public int Total;
		public List<int> Tokens = new List<int>();
		public string Channel;
		public DateTime Started;
		public TimeSpan Duration;
		
		private Random random = new Random();
		
		private int[] large = new int[5] {25,50,100,200,250};
		private int[] small = new int[9] {1,2,3,4,5,6,7,8,9};
		
		public CountdownRound(int largeCount, int smallCount, int seconds)
		{
			Started = DateTime.Now;
			Duration = TimeSpan.FromSeconds(seconds);
			Total = random.Next(1000);	
			List<int> largeList = new List<int>(large);
			List<int> smallList = new List<int>(small);
			for (int i = 0; i < largeCount; i++) 
			{
				int index = random.Next(largeList.Count);
				Tokens.Add(largeList[index]);
				largeList.RemoveAt(index);
			}
			for (int i = 0; i < smallCount; i++) 
			{
				int index = random.Next(smallList.Count);
				Tokens.Add(smallList[index]);
				smallList.RemoveAt(index);
			}
			Tokens.Sort();
		}
		
		public bool Expired()
		{
			return (Started + Duration < DateTime.Now);	
		}
		
		public bool IsExpressionValid(string expression)
		{
			String[] split = expression.Split(' ','/','+','*','-','(',')');
			List<int> tokenCopy = new List<int>(Tokens);
			foreach (string s in split)
			{
				int result;
				if (Int32.TryParse(s, out result))
				{
					if (!tokenCopy.Contains(result)) return false;	
					tokenCopy.Remove(result);
				}
			}
			return true;
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Total: ");
			sb.Append(Total);
			sb.Append(" Using: ");
			foreach (int i in Tokens)
			{
				sb.Append(i);
				sb.Append(" ");
			}
			sb.Append("You have ");
			sb.Append((int)(Duration - (DateTime.Now - Started)).TotalSeconds);
			sb.Append(" left!");
				
			return sb.ToString();
		}
	}
	
	public class ThetisCountdown : IThetisPlugin
	{
		public ThetisCountdown ()
		{
		}
		
		Dictionary<string,int> guesses;
		IThetisPluginHost host;
		CountdownRound activeRound; // TODO This needs to work for multiple channels
		
		#region Maths Evaluation
		private bool isExpressionAllowed(string expression)
		{
			string allowed = "+-/* 1234567890()";
			for (int i = 0; i < expression.Length; i++) {
				if (!allowed.Contains(expression.Substring(i,1))) return false;
			}
			return true;
		}
		
		private int Evaluate(string expression)
		{
			
			if (!isExpressionAllowed(expression)) 
			{
				throw new InvalidExpressionException();
				return 0;	
			}
			
			string source = @"
			class MyMath
			{
			    public static int Evaluate()
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
			int result = (int)method.Invoke(null,null);
			return result;
		}
		#endregion	
		
		private void EndRound()
		{
			if (guesses.Count == 0) 
			{
				host.SendToChannel(MessageType.Message,	activeRound.Channel, "You all sucked");
			}
			else 
			{
				KeyValuePair<String, int> best;
				foreach (KeyValuePair<String, int> kvp in guesses)
				{
					if (best.Key == null) 
					{
						best = kvp;
					}
					else 
					{
						if (Math.Abs(kvp.Value - activeRound.Total) < Math.Abs(best.Value - activeRound.Total)) best = kvp;	
						
					}
					
					
				
				}
				host.SendToChannel(MessageType.Message,	activeRound.Channel, String.Format("{0} won with {1}", best.Key, best.Value));
			}
			activeRound = null;
		}
		
		#region IThetisPlugin implementation
		public PluginResponse ChannelMessageReceived (MessageData message)
		{
			PluginResponse toReturn = new PluginResponse();
			if (message.Direct)
			{
				if (message.LowerCaseMessage.StartsWith("countdown"))
				{	
			
					toReturn.Claimed = true;
					if (activeRound != null)
					{
						host.SendToChannel(MessageType.Message,message.Channel,String.Format("Sorry, theres a game in progress. ({0})", activeRound.ToString()));	
					}
					else
					{
						activeRound = new CountdownRound(3,5,30);
						activeRound.Channel = message.Channel;
						guesses = new Dictionary<string, int>();
						host.SendToChannel(MessageType.Message, message.Channel, activeRound.ToString());
					}
				}
				else if (activeRound != null) // no game on
				{
					int result;
					try 
					{
						result = Evaluate(message.LowerCaseMessage);
						
					}
					catch
					{
						// not an expression
						return toReturn;
					}
					
					if (activeRound.IsExpressionValid(message.LowerCaseMessage))
					{
						toReturn.Claimed = true;
						host.SendToChannel(MessageType.Message, activeRound.Channel, String.Format("{0} ({1} from total)", result, Math.Abs(result - activeRound.Total)));
						if (result == activeRound.Total) //HOORAY
						{
							guesses[message.SentFrom.Nick] = result;
							EndRound(); //end round immediately
							
						}
						else if (guesses.ContainsKey(message.SentFrom.Nick))
						{
							if (Math.Abs(result - activeRound.Total) < Math.Abs(guesses[message.SentFrom.Nick] - activeRound.Total))
							{
								guesses[message.SentFrom.Nick] = result;		
							}
						}
						else
						{
							guesses[message.SentFrom.Nick] = result;	
						}
						
					}
					
					
				}
			}
			return toReturn;
		}

		public void Closing ()
		{
			
		}

		public void Tick ()
		{
			if (activeRound != null && activeRound.Expired()) EndRound();
		}

		public void Init ()
		{
			
		}

		public bool ForceSave ()
		{
			return true;
		}

		public string GetHelp (string command)
		{
			return null;
		}

		public IThetisPluginHost Host {
			set {
				host = value;
			}
		}

		public string Name {
			get {
				return "Countdown";
			}
		}

		public int Priority {
			get {
				return 0;
			}
		}

		public bool IgnoreClaimed {
			get {
				return true;
			}
		}
		#endregion
	}
}

