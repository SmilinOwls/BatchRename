using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BatchRename
{
    public class LowerCaseRule : IRule
    {
        public RuleFormat Format { get; set; }

        public LowerCaseRule()
        {
            Format = new RuleFormat();
        }

        public bool EditFormat()
        {
            return false;
        }

        public RuleParameterEditDialog EditDialog()
        {
            return null;
        }

        public string RuleName => "Lower.Case.Rule";

        public string ReName(string name, bool isFile = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            var Name = "";
            Regex regex = new Regex(@"\s+");

            if (isFile)
            {
                var lastDot = name.LastIndexOf('.');
                if (lastDot == -1) return "";
                var extension = name.Substring(lastDot + 1);
                Name = $"{regex.Replace(name.Substring(0, lastDot).Trim(), "").ToLower()}.{extension}";
            }
            else
                Name = regex.Replace(name.Trim(), "").ToLower();

            return Name;
        }
        public RuleFormat GetFormat()
        {
            return Format;
        }

        public IRule Instance()
        {
            LowerCaseRule rule = new LowerCaseRule();
            rule.Format = (RuleFormat)this.Format.Clone();
            return rule;
        }
    }
}
