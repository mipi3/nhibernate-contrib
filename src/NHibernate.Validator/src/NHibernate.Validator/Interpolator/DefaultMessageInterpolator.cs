using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using NHibernate.Util;
using NHibernate.Validator.Engine;
using NHibernate.Validator.Exceptions;

namespace NHibernate.Validator.Interpolator
{
	[Serializable]
	public class DefaultMessageInterpolator : IMessageInterpolator, ISerializable
	{
		private readonly Dictionary<string, object> attributeParameters = new Dictionary<string, object>();
		private string attributeMessage;

		[NonSerialized] private CultureInfo culture = CultureInfo.CurrentUICulture;
		[NonSerialized] private ResourceManager defaultMessageBundle;
		[NonSerialized] private string interpolateMessage;
		[NonSerialized] private ResourceManager messageBundle;

		public string AttributeMessage
		{
			get { return attributeMessage; }
		}

		public DefaultMessageInterpolator()
		{
		}

		public DefaultMessageInterpolator (SerializationInfo info, StreamingContext context)
		{
			attributeParameters = (Dictionary<string, object>) info.GetValue("attributeParameters",typeof(Dictionary<string, object>) );
			attributeMessage = (string) info.GetValue("message",typeof(string));
		}

		#region IMessageInterpolator Members

		public string Interpolate(string message, object entity, IValidator validator, IMessageInterpolator defaultInterpolator)
		{
			bool same = attributeMessage.Equals(message);
            if (same && interpolateMessage != null && !message.Contains("${"))
			{
				return interpolateMessage; //short cut
			}

			string result;
			result = Replace(message,entity);
			if (same)
			{
				interpolateMessage = result; //short cut in next iteration
			}
			return result;
		}

		#endregion

		public void Initialize(ResourceManager messageBundle, ResourceManager defaultMessageBundle, CultureInfo culture)
		{
			this.culture = culture ?? CultureInfo.CurrentCulture;
			this.messageBundle = messageBundle;
			this.defaultMessageBundle = defaultMessageBundle;
		}

		public void Initialize(Attribute attribute)
		{
			//Get all parametters of the Attribute: the name and their values.
			//For example:
			//In LengthAttribute the parametter are Min and Max.
			System.Type clazz = attribute.GetType();

			IRuleArgs ruleArgs = attribute as IRuleArgs;

			if (ruleArgs == null)
			{
				throw new ArgumentException("Attribute " + clazz + " doesn't implement IRuleArgs interface.");
			}
			else
			{
				if (ruleArgs.Message == null)
				{
					throw new ArgumentException(string.Format("The value of the message in {0} attribute is null (nothing for VB.Net Users). Add some message ", clazz) +
"on the attribute to solve this issue. For Example:\n" +
"- private string message = \"Error on property Foo\";\n" +
"Or you can use interpolators with resources files:\n" +
"-private string message = \"{validator.MyValidatorMessage}\";");
				}
				
				attributeMessage = ruleArgs.Message;
			}

			foreach (PropertyInfo property in clazz.GetProperties())
			{
				attributeParameters.Add(property.Name.ToLowerInvariant(), property.GetValue(attribute, null));
			}
		}

		private string Replace(string message, object entity)
		{
			var tokens = new StringTokenizer(message, "#${}", true);
			var buf = new StringBuilder(100);
			var escaped = false;
			var el = false;
			var isMember = false;

			IEnumerator ie = tokens.GetEnumerator();

			while (ie.MoveNext())
			{
				string token = (string) ie.Current;

				if (!escaped && "#".Equals(token))
				{
					el = true;
				}

				if(!el && ("$".Equals(token)))
				{
					isMember = true;
				}
				else if("}".Equals(token) && isMember)
				{
					isMember = false;
				}
				if (!el && "{".Equals(token))
				{
					escaped = true;
				}
				else if (escaped && "}".Equals(token))
				{
					escaped = false;
				}
				else if (!escaped)
				{
					if ("{".Equals(token))
					{
						el = false;
					}

					if(!"$".Equals(token)) buf.Append(token);
				}
				else if(!isMember)
				{
					object variable;
					if (attributeParameters.TryGetValue(token.ToLowerInvariant(), out variable))
					{
						buf.Append(variable);
					}
					else
					{
						string _string = null;
						try
						{
							_string = messageBundle != null ? messageBundle.GetString(token, culture) : null;
						}
						catch (MissingManifestResourceException)
						{
							//give a second chance with the default resource bundle
						}
						if (_string == null)
						{
							_string = defaultMessageBundle.GetString(token, culture);
							// in this case we don't catch the MissingManifestResourceException because
							// we are sure that we DefaultValidatorMessages.resx is an embedded resource
						}
						if (_string == null)
						{
							buf.Append('{').Append(token).Append('}');
						}
						else
						{
							buf.Append(Replace(_string,entity));
						}
					}
				}
				else
				{
					ReplaceValue(buf, entity, token);
				}
			}
			return buf.ToString();
		}

		/// <summary>
		/// Override this method to obtain flexibility.
		/// The default interpolator can replace the message with public property values.
		/// </summary>
		/// <param name="buffer">Current buffer where the final string message is written.</param>
		/// <param name="entity">Entity or value</param>
		/// <param name="propertyName">Property name to be used.</param>
		protected void ReplaceValue(StringBuilder buffer, object entity, string propertyName)
		{
			if (!propertyName.Contains("."))
			{
				var property = entity.GetType().GetProperty(propertyName);
				if (property == null) throw new InvalidPropertyNameException(propertyName, entity.GetType());

				var value = property.GetValue(entity, null);
				buffer.Append(value);
			}
			else
			{
				var membersChain = propertyName.Split('.');
				object value = entity;
				foreach (var memberName in membersChain)
				{
					var property = value.GetType().GetProperty(memberName);
					if (property == null) throw new InvalidPropertyNameException(memberName, entity.GetType());
					value = property.GetValue(value, null);
				}
				if(value != null)
					buffer.Append(value);
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//private readonly Dictionary<string, object> attributeParameters = new Dictionary<string, object>();
			//private string attributeMessage;

			info.AddValue("attributeParameters",attributeParameters );
			info.AddValue("message",attributeMessage );
		}
	}
}