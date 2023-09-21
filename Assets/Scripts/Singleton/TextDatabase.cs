using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class TextDatabase : Singleton<TextDatabase>
{
    public Dictionary<string, string> txtDB;

    protected override void Awake()
    {
        base.Awake();
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        TextAsset xml = Resources.Load<TextAsset>("Text Database");

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml.text);

        XmlNodeList nodes = doc.DocumentElement.SelectNodes("descendant::text");
        txtDB = new Dictionary<string, string>();

        foreach (XmlNode node in nodes)
        {
            txtDB.Add(node.ParentNode.Name + "/" + node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText);
        }
    }
}
