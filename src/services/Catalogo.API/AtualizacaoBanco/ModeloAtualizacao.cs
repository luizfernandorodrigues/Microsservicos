using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Catalogo.API.AtualizacaoBanco
{
    [Serializable]
    [XmlRoot(IsNullable = false, ElementName = "atualizacao", Namespace = "atualizacao")]
    public class ModeloAtualizacao
    {
        [XmlElement(ElementName = "script")]
        public List<Script> ListaScripts { get; set; }

        [XmlElement(ElementName = "versao")]
        public Versao Versao { get; set; }
    }
}
