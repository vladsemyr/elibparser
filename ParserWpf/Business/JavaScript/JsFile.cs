using System;
using System.Linq;
using System.Reflection;

namespace ParserWpf.Business
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class JsFilenameAttribute : System.Attribute
    {
        private string filename;

        public string Filename { get => filename; }

        public JsFilenameAttribute(string name)
        {
            this.filename = name;
        }
    }

    public enum JsFile
    {
        [JsFilenameAttribute("result.js")]
        Result,

        [JsFilenameAttribute("result_back.js")]
        ResultBack,

        [JsFilenameAttribute("robot.js")]
        Robot,

        [JsFilenameAttribute("search.js")]
        Search
    }

    class JsFileToFilenameConvertor
    {
        public static string Convert(JsFile t)
        {
            MemberInfo memberInfo = typeof(JsFile).GetMember(t.ToString())
                                              .FirstOrDefault();

            if (memberInfo != null)
            {
                JsFilenameAttribute attribute = (JsFilenameAttribute)
                             memberInfo.GetCustomAttributes(typeof(JsFilenameAttribute), false)
                                       .FirstOrDefault();
                return attribute.Filename;
            }

            return null;
        }
    }

}