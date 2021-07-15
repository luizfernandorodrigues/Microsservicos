using System.Xml.Serialization;

namespace Catalogo.API.AtualizacaoBanco
{
    [XmlRoot(IsNullable = false, ElementName = "script")]
    public class Script
    {
        [XmlText()]
        public string Sql { get; set; }

        [XmlAttribute(AttributeName = "nome")]
        public string Nome { get; set; }
        public bool TemSql
        {
            get { return !string.IsNullOrEmpty(Sql); }
        }
        public bool TemNome
        {
            get { return !string.IsNullOrEmpty(Nome); }
        }
    }
}
