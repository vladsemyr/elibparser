using System.Linq;
using System.Reflection;

namespace ParserWpf.Business.JavaScript
{
    public static class JsFileToFilenameConvertor
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
                return attribute?.Filename;
            }

            return null;
        }
    }
}