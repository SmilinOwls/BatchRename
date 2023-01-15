using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public class PascalCaseRule : IRule
    {
        public RuleFormat Format { get; set; }

        public PascalCaseRule()
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

        public string RuleName => "Pascal.Case.Rule";

        public string ReName(string name, bool isFile = true)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            var Name = "";

            if (isFile)
            {
                var lastDot = name.LastIndexOf('.');
                if (lastDot == -1) return "";

                var extension = name.Substring(lastDot + 1);
                if (string.IsNullOrEmpty(extension)) return "";

                Name = $"{RuleHelper.ToPascalCase(name.Substring(0, lastDot))}.{extension}";
            }
            else
                Name = RuleHelper.ToPascalCase(name);

            return Name;
        }

        public RuleFormat GetFormat()
        {
            return Format;
        }

        public void SetFormat(RuleFormat ruleFormat)
        {
            Format = ruleFormat;
        }

        public IRule Instance()
        {
            PascalCaseRule rule = new PascalCaseRule();
            rule.Format = (RuleFormat)this.Format.Clone();
            return rule;
        }
    }
}
