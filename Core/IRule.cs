using Core.Format;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
    public interface IRule
    {
        // Edit
        public bool EditFormat();

        public RuleParameterEditDialog EditDialog();

        // Name of Rule
        public string RuleName { get; }

        // Rename of File or Folder
        public string ReName(string name, bool isFile = true);

        // Getter & Setter
        public RuleFormat GetFormat();

        public void SetFormat(RuleFormat ruleFormat)
        {
            return;
        }

        public IRule Instance();

    }

    public class RuleFormat : ICloneable
    {
        public RuleFormat()
        {
            Type = "";
            Parameter = new List<string>();
            Result = "";

        }

        public string Type { get; set; }
        public List<string> Parameter { get; set; }
        public string Result { get; set; }

        public object Clone()
        {
            var result = (RuleFormat) MemberwiseClone();
            result.Parameter = Parameter.Select(parameter => (string)parameter.Clone()).ToList();
            return result;
        }
    }

    public class ProjectFormat
    {
        public List<FileFormat> File { get; set; }
        public List<FolderFormat> Folder { get; set; }
        public List<RuleFormat> Rule { get; set; }
    }

    public class WindowPositionProperty
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }

    public class LastUseProject
    {
        public int Type { get; set; } // 0: File, 1: Folder
        public WindowPositionProperty WindowPositionProperty { get; set; }
        public List<RuleFormat> Rule { get; set; }
    }
}
