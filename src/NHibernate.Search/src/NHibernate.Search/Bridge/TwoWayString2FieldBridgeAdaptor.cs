using System;
using Lucene.Net.Documents;

namespace NHibernate.Search.Bridge
{
    /// <summary>
    /// Bridge to use a TwoWayStringBridge as a TwoWayFieldBridge
    /// </summary>
    /// TODO: Use Generics to avoid double declaration of stringBridge 
    public class TwoWayString2FieldBridgeAdaptor : String2FieldBridgeAdaptor, ITwoWayFieldBridge
    {
        private readonly ITwoWayStringBridge stringBridge;

        public TwoWayString2FieldBridgeAdaptor(ITwoWayStringBridge stringBridge) : base(stringBridge)
        {
            this.stringBridge = stringBridge;
        }

        #region ITwoWayFieldBridge Members

        public Object Get(String name, Document document)
        {
            Field field = document.GetField(name);
            if (field == null)
                return null;
            return stringBridge.StringToObject(field.StringValue());
        }

        public string ObjectToString(object obj)
        {
            return stringBridge.ObjectToString(obj);
        }

        #endregion
    }
}