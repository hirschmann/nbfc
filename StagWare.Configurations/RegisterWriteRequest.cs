using System;

namespace StagWare.FanControl.Configurations
{
    public class RegisterWriteRequest : ICloneable
    {
        #region Properties

        public RegisterWriteMode WriteMode { get; set; }
        public int Register { get; set; }
        public int Value { get; set; } 

        #endregion

        #region ICloneable implementation

        public object Clone()
        {
            return new RegisterWriteRequest()
            {
                Register = this.Register,
                Value = this.Value,
                WriteMode = this.WriteMode
            };
        } 

        #endregion
    }
}
