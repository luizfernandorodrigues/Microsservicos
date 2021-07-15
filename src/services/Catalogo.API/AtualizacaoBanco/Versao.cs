using System.Xml.Serialization;

namespace Catalogo.API.AtualizacaoBanco
{
    [XmlRoot(IsNullable = false, ElementName = "versao")]
    public class Versao
    {
        [XmlAttribute(AttributeName = "numero")]
        public int Numero { get; set; }

        [XmlAttribute(AttributeName = "guid")]
        public string Guid { get; set; }
    }
}
