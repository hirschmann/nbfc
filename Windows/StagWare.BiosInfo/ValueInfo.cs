using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.BiosInfo
{
    public class ValueInfo
    {
        public ValueInfo(string valueName, InfoValueType valueType)
        {
            this.ValueName = valueName;
            this.ValueType = valueType;
        }

        public string ValueName { get; private set; }
        public InfoValueType ValueType { get; private set; }
    }
}
