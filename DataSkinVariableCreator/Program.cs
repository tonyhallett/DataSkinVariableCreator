using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataSkinVariableCreator
{
    
    public delegate string SkinXmlStringProvider(string text);
    class Program
    {
        static void Main(string[] args)
        {

            var outPath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "skinData.txt";
            WriteVariablesToScriptElement(outPath, (skinText) =>
            {
                var startIndex = skinText.IndexOf("====================") + 20;
                var endIndex = skinText.IndexOf("*/", startIndex);
                return skinText.Substring(startIndex, endIndex);
            });

        }
        static FileInfo GetSkinFile()
        {
            return new FileInfo(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "bskin.txt");
        }
        static XDocument GetSkinXml(string skinXml)
        {
            return XDocument.Parse(skinXml);
        }
        static XElement GetSkinRoot(SkinXmlStringProvider skinXmlStringProvider)
        {
            return GetSkinXml("<root>" + skinXmlStringProvider(GetSkinFile().OpenText().ReadToEnd()) + "</root>").Root;
        }
        static IEnumerable<XElement> GetVariables(SkinXmlStringProvider skinXmlStringProvider)
        {
            return GetSkinRoot(skinXmlStringProvider).Descendants("Variable");
        }
        static void WriteVariablesToScriptElement(string path, SkinXmlStringProvider skinXmlStringProvider)
        {
            StreamWriter writer = new StreamWriter(path);
            var varName= "themeSkinVariables";
            writer.WriteLine("<script type='application/javascript'>");
            writer.WriteLine($"var {varName} ={{}};");
            foreach(var variable in GetVariables(skinXmlStringProvider))
            {
                var variableName = variable.Attribute("name").Value;
                writer.WriteLine($"{varName}['{variableName}']='{GetDataTag(variableName)}';");
            }

            writer.WriteLine("</script>");
            writer.Close();
        }
        static string GetDataTag(string variableName)
        {
            var parts=variableName.Split(new[] { "." }, StringSplitOptions.None);
            var varSuffix = "";
            for (var i = 0; i < parts.Length; i++){
                varSuffix += parts[i];
                if (i != parts.Length-1)
                {
                    varSuffix += "_";
                }
            }
            return "<data:skin.vars." + varSuffix + "/>";
        }
    }
}
