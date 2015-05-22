using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Configuration;
using System.Text.RegularExpressions;


namespace MyCmn
{
    public class CpuSize
    {
        public double Value { get; set; }
        public CpuSizeEnum Unit { get; set; }

        public CpuSize()
        {

        }
        public CpuSize(string Description)
            : this()
        {
            Regex rex = new Regex(@"\d+[\.]?\d*", RegexOptions.Compiled);

            var res = rex.Match(Description);
            if (res.Success == false) return;

            this.Value = res.Value.AsDouble();

            this.Unit = Description.Slice(Description.IndexOf(res.Value) + res.Value.Length).Trim().ToEnum<CpuSizeEnum>();
        }

        public override string ToString()
        {
            return string.Format(@"{0} {1}", Value.ToString("0.##"), Unit.ToString());
        }

        public double ToBytesValue()
        {
            if (Unit.HasValue() == false) return 0;
            if (Unit == CpuSizeEnum.Bytes)
            {
                return this.Value;
            }

            var cur = new CpuSize() { Value = this.Value, Unit = this.Unit };

            cur.Value = cur.Value * 1024;
            cur.Unit = (cur.Unit.AsInt() - 1).ToEnum<CpuSizeEnum>();
            return cur.ToBytesValue();
        }

        public string ToFixedString()
        {
            if (Unit == CpuSizeEnum.TB || Value <= 1024)
            {
                return ToString();
            }

            var cur = new CpuSize() { Value = this.Value, Unit = this.Unit };

            if (cur.Value > 1024)
            {
                cur.Value = cur.Value / 1024;
                cur.Unit = (cur.Unit.AsInt() + 1).ToEnum<CpuSizeEnum>();
                return cur.ToFixedString();
            }
            return ToString();
        }
    }

}
