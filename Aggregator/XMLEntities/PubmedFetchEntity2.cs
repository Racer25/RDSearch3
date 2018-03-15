
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace WebCrawler2
{
    [XmlRoot(ElementName = "journal-id")]
    public class Journalid
    {
        [XmlAttribute(AttributeName = "journal-id-type")]
        public string Journalidtype { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "journal-title-group")]
    public class Journaltitlegroup
    {
        [XmlElement(ElementName = "journal-title")]
        public string Journaltitle { get; set; }
    }

    [XmlRoot(ElementName = "issn")]
    public class Issn
    {
        [XmlAttribute(AttributeName = "pub-type")]
        public string Pubtype { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "journal-meta")]
    public class Journalmeta
    {
        [XmlElement(ElementName = "journal-id")]
        public List<Journalid> Journalid { get; set; }
        [XmlElement(ElementName = "journal-title-group")]
        public Journaltitlegroup Journaltitlegroup { get; set; }
        [XmlElement(ElementName = "issn")]
        public List<Issn> Issn { get; set; }
        [XmlElement(ElementName = "publisher")]
        public Publisher Publisher { get; set; }
    }

    [XmlRoot(ElementName = "article-id")]
    public class Articleid
    {
        [XmlAttribute(AttributeName = "pub-id-type")]
        public string Pubidtype { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "subj-group")]
    public class Subjgroup
    {
        [XmlElement(ElementName = "subject")]
        public string Subject { get; set; }
        [XmlAttribute(AttributeName = "subj-group-type")]
        public string Subjgrouptype { get; set; }
    }

    [XmlRoot(ElementName = "article-categories")]
    public class Articlecategories
    {
        [XmlElement(ElementName = "subj-group")]
        public Subjgroup Subjgroup { get; set; }
    }

    [XmlRoot(ElementName = "title-group")]
    public class Titlegroup
    {
        [XmlElement(ElementName = "article-title")]
        public string Articletitle { get; set; }
    }

    [XmlRoot(ElementName = "name")]
    public class Name
    {
        [XmlElement(ElementName = "surname")]
        public string Surname { get; set; }
        [XmlElement(ElementName = "given-names")]
        public string Givennames { get; set; }
        [XmlElement(ElementName = "suffix")]
        public string Suffix { get; set; }
    }

    [XmlRoot(ElementName = "xref")]
    public class Xref
    {
        [XmlAttribute(AttributeName = "ref-type")]
        public string Reftype { get; set; }
        [XmlAttribute(AttributeName = "rid")]
        public string Rid { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "contrib")]
    public class Contrib
    {
        [XmlElement(ElementName = "name")]
        public Name Name { get; set; }
        [XmlElement(ElementName = "xref")]
        public List<Xref> Xref { get; set; }
        [XmlAttribute(AttributeName = "contrib-type")]
        public string Contribtype { get; set; }
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }
    }

    [XmlRoot(ElementName = "contrib-group")]
    public class Contribgroup
    {
        [XmlElement(ElementName = "contrib")]
        public List<Contrib> Contrib { get; set; }
    }

    [XmlRoot(ElementName = "aff")]
    public class Aff
    {
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "corresp")]
    public class Corresp
    {
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "fn")]
    public class Fn
    {
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "fn-type")]
        public string Fntype { get; set; }
        [XmlElement(ElementName = "p")]
        public List<P> P { get; set; }
    }

    [XmlRoot(ElementName = "author-notes")]
    public class Authornotes
    {
        [XmlElement(ElementName = "corresp")]
        public Corresp Corresp { get; set; }
        [XmlElement(ElementName = "fn")]
        public List<Fn> Fn { get; set; }
    }

    [XmlRoot(ElementName = "pub-date")]
    public class Pubdate
    {
        [XmlElement(ElementName = "day")]
        public string Day { get; set; }
        [XmlElement(ElementName = "month")]
        public string Month { get; set; }
        [XmlElement(ElementName = "year")]
        public string Year { get; set; }
        [XmlAttribute(AttributeName = "pub-type")]
        public string Pubtype { get; set; }
    }

    [XmlRoot(ElementName = "ext-link")]
    public class Extlink
    {
        [XmlAttribute(AttributeName = "ext-link-type")]
        public string Extlinktype { get; set; }
        [XmlAttribute(AttributeName = "href", Namespace = "http://www.w3.org/1999/xlink")]
        public string Href { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "license-p")]
    public class Licensep
    {
        [XmlElement(ElementName = "ext-link")]
        public Extlink Extlink { get; set; }
    }

    [XmlRoot(ElementName = "license")]
    public class License
    {
        [XmlElement(ElementName = "license-p")]
        public Licensep Licensep { get; set; }
        [XmlAttribute(AttributeName = "license-type")]
        public string Licensetype { get; set; }
        [XmlAttribute(AttributeName = "href", Namespace = "http://www.w3.org/1999/xlink")]
        public string Href { get; set; }
    }

    [XmlRoot(ElementName = "permissions")]
    public class Permissions
    {
        [XmlElement(ElementName = "license")]
        public License License { get; set; }
        [XmlElement(ElementName = "copyright-statement")]
        public string Copyrightstatement { get; set; }
        [XmlElement(ElementName = "copyright-year")]
        public string Copyrightyear { get; set; }
    }

    [XmlRoot(ElementName = "p")]
    public class P
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlElement(ElementName = "graphic")]
        public Graphic Graphic { get; set; }
        [XmlElement(ElementName = "xref")]
        public List<Xref> Xref { get; set; }
        [XmlElement(ElementName = "sup")]
        public List<string> Sup { get; set; }
        [XmlElement(ElementName = "italic")]
        public Italic Italic { get; set; }
        [XmlElement(ElementName = "bold")]
        public string Bold { get; set; }
        [XmlElement(ElementName = "ext-link")]
        public Extlink Extlink { get; set; }
        [XmlElement(ElementName = "list")]
        public List List { get; set; }
        [XmlElement(ElementName = "sub")]
        public string Sub { get; set; }
        [XmlElement(ElementName = "sc")]
        public string Sc { get; set; }
        [XmlElement(ElementName = "table-wrap")]
        public Tablewrap Tablewrap { get; set; }
        [XmlElement(ElementName = "funding-source")]
        public List<Fundingsource> Fundingsource { get; set; }
    }

    [XmlRoot(ElementName = "abstract")]
    public class Abstract
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "p")]
        public List<P> P { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "abstract-type")]
        public string Abstracttype { get; set; }
    }

    [XmlRoot(ElementName = "graphic")]
    public class Graphic
    {
        [XmlAttribute(AttributeName = "href", Namespace = "http://www.w3.org/1999/xlink")]
        public string Href { get; set; }
        [XmlAttribute(AttributeName = "position")]
        public string Position { get; set; }
        [XmlAttribute(AttributeName = "orientation")]
        public string Orientation { get; set; }
    }

    [XmlRoot(ElementName = "article-meta")]
    public class Articlemeta
    {
        [XmlElement(ElementName = "article-id")]
        public List<Articleid> Articleid { get; set; }
        [XmlElement(ElementName = "article-categories")]
        public Articlecategories Articlecategories { get; set; }
        [XmlElement(ElementName = "title-group")]
        public Titlegroup Titlegroup { get; set; }
        [XmlElement(ElementName = "contrib-group")]
        public Contribgroup Contribgroup { get; set; }
        [XmlElement(ElementName = "aff")]
        public List<Aff> Aff { get; set; }
        [XmlElement(ElementName = "author-notes")]
        public Authornotes Authornotes { get; set; }
        [XmlElement(ElementName = "pub-date")]
        public List<Pubdate> Pubdate { get; set; }
        [XmlElement(ElementName = "volume")]
        public string Volume { get; set; }
        [XmlElement(ElementName = "issue")]
        public string Issue { get; set; }
        [XmlElement(ElementName = "fpage")]
        public string Fpage { get; set; }
        [XmlElement(ElementName = "lpage")]
        public string Lpage { get; set; }
        [XmlElement(ElementName = "permissions")]
        public Permissions Permissions { get; set; }
        [XmlElement(ElementName = "abstract")]
        public List<Abstract> Abstract { get; set; }
        [XmlElement(ElementName = "history")]
        public History History { get; set; }
        [XmlElement(ElementName = "kwd-group")]
        public Kwdgroup Kwdgroup { get; set; }
    }

    [XmlRoot(ElementName = "front")]
    public class Front
    {
        [XmlElement(ElementName = "journal-meta")]
        public Journalmeta Journalmeta { get; set; }
        [XmlElement(ElementName = "article-meta")]
        public Articlemeta Articlemeta { get; set; }
    }

    [XmlRoot(ElementName = "sec")]
    public class Sec
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "p")]
        public List<P> P { get; set; }
        [XmlAttribute(AttributeName = "sec-type")]
        public string Sectype { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "sec")]
        public List<Sec> sec { get; set; }
        [XmlElement(ElementName = "supplementary-material")]
        public List<Supplementarymaterial> Supplementarymaterial { get; set; }
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
    }

    [XmlRoot(ElementName = "italic")]
    public class Italic
    {
        [XmlElement(ElementName = "sup")]
        public string Sup { get; set; }
    }

    [XmlRoot(ElementName = "media")]
    public class Media
    {
        [XmlAttribute(AttributeName = "href", Namespace = "http://www.w3.org/1999/xlink")]
        public string Href { get; set; }
        [XmlAttribute(AttributeName = "orientation")]
        public string Orientation { get; set; }
        [XmlAttribute(AttributeName = "type", Namespace = "http://www.w3.org/1999/xlink")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "position")]
        public string Position { get; set; }
    }

    [XmlRoot(ElementName = "supplementary-material")]
    public class Supplementarymaterial
    {
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlElement(ElementName = "media")]
        public Media Media { get; set; }
        [XmlAttribute(AttributeName = "content-type")]
        public string Contenttype { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "body")]
    public class Body
    {
        [XmlElement(ElementName = "sec")]
        public List<Sec> Sec { get; set; }
    }

    [XmlRoot(ElementName = "ack")]
    public class Ack
    {
        [XmlElement(ElementName = "p")]
        public string P { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
    }

    [XmlRoot(ElementName = "fn-group")]
    public class Fngroup
    {
        [XmlElement(ElementName = "fn")]
        public List<Fn> Fn { get; set; }
    }

    [XmlRoot(ElementName = "person-group")]
    public class Persongroup
    {
        [XmlElement(ElementName = "name")]
        public List<Name> Name { get; set; }
        [XmlAttribute(AttributeName = "person-group-type")]
        public string Persongrouptype { get; set; }
        [XmlElement(ElementName = "etal")]
        public string Etal { get; set; }
    }

    [XmlRoot(ElementName = "pub-id")]
    public class Pubid
    {
        [XmlAttribute(AttributeName = "pub-id-type")]
        public string Pubidtype { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "element-citation")]
    public class Elementcitation
    {
        [XmlElement(ElementName = "person-group")]
        public Persongroup Persongroup { get; set; }
        [XmlElement(ElementName = "year")]
        public string Year { get; set; }
        [XmlElement(ElementName = "source")]
        public string Source { get; set; }
        [XmlElement(ElementName = "volume")]
        public string Volume { get; set; }
        [XmlElement(ElementName = "fpage")]
        public string Fpage { get; set; }
        [XmlElement(ElementName = "pub-id")]
        public Pubid Pubid { get; set; }
        [XmlAttribute(AttributeName = "publication-type")]
        public string Publicationtype { get; set; }
        [XmlElement(ElementName = "lpage")]
        public string Lpage { get; set; }
        [XmlElement(ElementName = "comment")]
        public Comment Comment { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "article-title")]
        public Articletitle Articletitle { get; set; }
        [XmlElement(ElementName = "issue")]
        public string Issue { get; set; }
    }

    [XmlRoot(ElementName = "ref")]
    public class Ref
    {
        [XmlElement(ElementName = "element-citation")]
        public Elementcitation Elementcitation { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlElement(ElementName = "mixed-citation")]
        public Mixedcitation Mixedcitation { get; set; }
    }

    [XmlRoot(ElementName = "comment")]
    public class Comment
    {
        [XmlElement(ElementName = "ext-link")]
        public Extlink Extlink { get; set; }
    }

    [XmlRoot(ElementName = "ref-list")]
    public class Reflist
    {
        [XmlElement(ElementName = "ref")]
        public List<Ref> Ref { get; set; }
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "back")]
    public class Back
    {
        [XmlElement(ElementName = "ack")]
        public Ack Ack { get; set; }
        [XmlElement(ElementName = "fn-group")]
        public Fngroup Fngroup { get; set; }
        [XmlElement(ElementName = "ref-list")]
        public Reflist Reflist { get; set; }
    }

    [XmlRoot(ElementName = "caption")]
    public class Caption
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "p")]
        public List<P> P { get; set; }
    }

    [XmlRoot(ElementName = "fig")]
    public class Fig
    {
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlElement(ElementName = "caption")]
        public Caption Caption { get; set; }
        [XmlElement(ElementName = "graphic")]
        public Graphic Graphic { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "orientation")]
        public string Orientation { get; set; }
        [XmlAttribute(AttributeName = "position")]
        public string Position { get; set; }
    }

    [XmlRoot(ElementName = "list-item")]
    public class Listitem
    {
        [XmlElement(ElementName = "p")]
        public P P { get; set; }
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "list")]
    public class List
    {
        [XmlElement(ElementName = "list-item")]
        public List<Listitem> Listitem { get; set; }
        [XmlAttribute(AttributeName = "list-type")]
        public string Listtype { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "boxed-text")]
    public class Boxedtext
    {
        [XmlElement(ElementName = "caption")]
        public Caption Caption { get; set; }
        [XmlElement(ElementName = "list")]
        public List List { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "position")]
        public string Position { get; set; }
        [XmlAttribute(AttributeName = "orientation")]
        public string Orientation { get; set; }
    }

    [XmlRoot(ElementName = "floats-group")]
    public class Floatsgroup
    {
        [XmlElement(ElementName = "fig")]
        public List<Fig> Fig { get; set; }
        [XmlElement(ElementName = "boxed-text")]
        public Boxedtext Boxedtext { get; set; }
    }

    [XmlRoot(ElementName = "article")]
    public class Article
    {
        [XmlElement(ElementName = "front")]
        public Front Front { get; set; }
        [XmlElement(ElementName = "body")]
        public Body Body { get; set; }
        [XmlElement(ElementName = "back")]
        public Back Back { get; set; }
        [XmlElement(ElementName = "floats-group")]
        public Floatsgroup Floatsgroup { get; set; }
        [XmlAttribute(AttributeName = "xlink", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xlink { get; set; }
        [XmlAttribute(AttributeName = "mml", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Mml { get; set; }
        [XmlAttribute(AttributeName = "article-type")]
        public string Articletype { get; set; }
    }

    [XmlRoot(ElementName = "publisher")]
    public class Publisher
    {
        [XmlElement(ElementName = "publisher-name")]
        public string Publishername { get; set; }
    }

    [XmlRoot(ElementName = "date")]
    public class Date
    {
        [XmlElement(ElementName = "day")]
        public string Day { get; set; }
        [XmlElement(ElementName = "month")]
        public string Month { get; set; }
        [XmlElement(ElementName = "year")]
        public string Year { get; set; }
        [XmlAttribute(AttributeName = "date-type")]
        public string Datetype { get; set; }
    }

    [XmlRoot(ElementName = "history")]
    public class History
    {
        [XmlElement(ElementName = "date")]
        public List<Date> Date { get; set; }
    }

    [XmlRoot(ElementName = "kwd-group")]
    public class Kwdgroup
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "kwd")]
        public List<string> Kwd { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "alt-text")]
    public class Alttext
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "th")]
    public class Th
    {
        [XmlElement(ElementName = "hr")]
        public string Hr { get; set; }
        [XmlAttribute(AttributeName = "colspan")]
        public string Colspan { get; set; }
        [XmlAttribute(AttributeName = "align")]
        public string Align { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "tr")]
    public class Tr
    {
        [XmlElement(ElementName = "th")]
        public List<Th> Th { get; set; }
        [XmlElement(ElementName = "td")]
        public List<Td> Td { get; set; }
    }

    [XmlRoot(ElementName = "thead")]
    public class Thead
    {
        [XmlElement(ElementName = "tr")]
        public List<Tr> Tr { get; set; }
    }

    [XmlRoot(ElementName = "td")]
    public class Td
    {
        [XmlElement(ElementName = "italic")]
        public List<string> Italic { get; set; }
        [XmlAttribute(AttributeName = "align")]
        public string Align { get; set; }
        [XmlElement(ElementName = "xref")]
        public List<Xref> Xref { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlElement(ElementName = "sub")]
        public string Sub { get; set; }
        [XmlElement(ElementName = "sc")]
        public string Sc { get; set; }
        [XmlAttribute(AttributeName = "colspan")]
        public string Colspan { get; set; }
    }

    [XmlRoot(ElementName = "tbody")]
    public class Tbody
    {
        [XmlElement(ElementName = "tr")]
        public List<Tr> Tr { get; set; }
    }

    [XmlRoot(ElementName = "table")]
    public class Table
    {
        [XmlElement(ElementName = "thead")]
        public Thead Thead { get; set; }
        [XmlElement(ElementName = "tbody")]
        public Tbody Tbody { get; set; }
        [XmlAttribute(AttributeName = "frame")]
        public string Frame { get; set; }
        [XmlAttribute(AttributeName = "rules")]
        public string Rules { get; set; }
    }

    [XmlRoot(ElementName = "table-wrap")]
    public class Tablewrap
    {
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }
        [XmlElement(ElementName = "caption")]
        public Caption Caption { get; set; }
        [XmlElement(ElementName = "alt-text")]
        public Alttext Alttext { get; set; }
        [XmlElement(ElementName = "table")]
        public Table Table { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "position")]
        public string Position { get; set; }
    }

    [XmlRoot(ElementName = "funding-source")]
    public class Fundingsource
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "article-title")]
    public class Articletitle
    {
        [XmlElement(ElementName = "italic")]
        public string Italic { get; set; }
    }

    [XmlRoot(ElementName = "mixed-citation")]
    public class Mixedcitation
    {
        [XmlAttribute(AttributeName = "publication-type")]
        public string Publicationtype { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "pmc-articleset")]
    public class Pmcarticleset
    {
        [XmlElement(ElementName = "article")]
        public List<Article> Article { get; set; }
    }

}
