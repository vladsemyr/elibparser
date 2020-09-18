namespace ParserWpf.Business.JavaScript
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class JsFilenameAttribute : System.Attribute
    {
        public string Filename { get; }

        public JsFilenameAttribute(string name)
        {
            Filename = name;
        }
    }
}