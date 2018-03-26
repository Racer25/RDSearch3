using ConfigurationJSON;
using MongoRepository.entities;
using MongoRepository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

namespace TestXMLSerializer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            List<string> maListe = new List<string>() {
                "5848491",
                "5846849",
               "5843454",
            "5738781",
            "5728969",
            "5736932",
            "5717919",
            "5637193",
            "5621198",
            "5803675",
            "5538086",
            "5467284",
            "5438510",
            "5448069",
            "5649998",
            "5417081",
            "5337308",
            "5725965",
            "5285484",
            "5055915",
            "5536113",
            "5009579",
            "5124792",
            "5308667",
            "4993965",
            "4974427",
            "5363503",
            "5342359",
            "4907005",
            "4909516",
            "5082049",
            "5082046",
            "4981530",
            "4977195",
            "4882106",
            "4918700",
            "4816695",
            "4739400",
            "4716319",
            "4790153",
            "4732252",
            "4679641",
            "4985963",
            "4899878",
            "4815887",
            "4636768",
            "4625247",
            "4484562",
            "4525909",
            "4431945",
            "4476386",
            "4407356",
            "4457776",
            "4398928",
            "4492820",
            "4369005",
            "4333262",
            "4418984",
            "4422428",
            "4443574",
            "4325684",
            "4602630",
            "4433391",
            "4284484",
            "5123092",
            "4542800",
            "4458069",
            "4533342",
            "4229774",
            "4212306",
            "4173870",
            "4186604",
            "4321877",
            "4234453",
            "4129077",
            "4046297",
            "4097238",
            "4037780",
            "4016635",
            "4691539",
            "4016852",
            "4382473",
            "3960408",
            "4369702",
            "4016461",
            "4096170",
            "4150212",
            "3902278",
            "3937153",
            "3956353",
            "4079032",
            "3864181",
            "3843285",
            "3790009",
            "3675257",
            "3824799",
            "3656567",
            "3645840",
            "3586689",
            "3598795",
            "4703041",
            "3590650",
            "3498206",
            "3544693",
            "4629840",
            "3573597",
            "3544459",
            "3458837",
            "3443643",
            "3461912",
            "3433827",
            "3597153",
            "3476993",
            "3430302",
            "3412717",
            "3353763",
            "3367317",
            "3329859",
            "3479702",
            "3335540",
            "3629555",
            "3261309",
            "3280808",
            "3657472",
            "3337896",
            "3263419",
            "3215551",
            "3201189",
            "3095535",
            "3070454",
            "3271574",
            "3173546",
            "3070575",
            "2995789",
            "3018903",
            "2994058",
            "3093914",
            "2924852",
            "2878625",
            "2878626",
            "2847649",
            "2990242",
            "3534347",
            "2825527",
            "2823713",
            "2994463",
            "2920385",
            "2765459",
            "2763065",
            "2994437",
            "4816642",
            "2724918",
            "2652636",
            "2565013",
            "2735164",
            "1971266",
            "4878392",
            "1955072",
            "2148085",
            "2395629",
            "1183444",
            "535354",
            "1872004",
            "519028",
            "1920658",
            "1850421",
            "1891915",
            "1850316",
            "1722578",
            "3454647",
            "504612",
            "1015339",
            "1014585",
            "1041082",
            "1911861",
            "1298705",
            "1001222",
            "498196",
            "1043565",
            "1083599",
            "1310506",
            "1310468",
            "1207761",
            "2070963",
            "2012771",
            "1889646",
            "1802131",
            "510970",
            "1997770",
            "511438"};
            List<long> maListe2 = maListe.Select(x => long.Parse(x)).Take(50).ToList();
            //List<long> maListe2 = maListe.Select(x => long.Parse(x)).Skip(Math.Max(0, maListe.Count - 100)).ToList();
            foreach (long i in maListe2)
            {
                Console.Write($"{i},");
            }
            Console.WriteLine();

            //PubmedCrawlerFetch2(0, 500, new Disease("10083", "C'est un test"), maListe2).Start();
            var myTask = DoTheRequestAsync2(maListe2, new Disease("10083", "C'est un test"));
            myTask.Start();
            myTask.Wait();


            Console.WriteLine("Finished :)");

            Console.ReadKey();

        }

        public static Task DoTheRequestAsync(List<long> maListe2, Disease disease)
        {
            return new Task(async () =>
            {
                //Initialization
                HttpClient client = new HttpClient();
                List<Publication> results = new List<Publication>();

                //URL and parameters
                string url = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=pmc";
                string ids = String.Join(", ", maListe2.ToArray());
                string jsonInString = JsonConvert.SerializeObject(new
                {
                    //db = "pmc",
                    retstart = $"{0}",
                    retmax = $"{500}",
                    rettype = "xml",
                    retmode = "xml",
                    api_key = $"{ConfigurationManager.Instance.config.API_Key}",
                    email = $"{ConfigurationManager.Instance.config.Email}",
                    tool = $"{ConfigurationManager.Instance.config.Tool}",
                    id = ids
                });

                var parameters = new Dictionary<string, string>{
                    { "db", "pmc" },
                    { "retstart", "0" },
                    { "retmax", "500" },
                    { "rettype", "xml" },
                    { "retmode", "xml" },
                    { "api_key", $"{ConfigurationManager.Instance.config.API_Key}" },
                    { "email", $"{ConfigurationManager.Instance.config.Email}" },
                    { "tool", $"{ConfigurationManager.Instance.config.Tool}" },
                    { "id", ids }
                };

                var urlContent = new FormUrlEncodedContent(parameters);
                Console.WriteLine(urlContent);

                //Make the request and deserialization
                XmlSerializer serializer = new XmlSerializer(typeof(Pmcarticleset));
                using (HttpResponseMessage res = await client.PostAsync(url, urlContent))
                using (HttpContent content = res.Content)
                using (var stream = await content.ReadAsStreamAsync())
                //using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))//1
                {
                    string stringRes = await content.ReadAsStringAsync();//2
                    var reader = new StreamReader(stream, Encoding.UTF8);//2
                    string resString = reader.ReadToEnd();//2
                    //var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });//1
                    try
                    {
                        //var pmcArticleSet = serializer.Deserialize(reader) as Pmcarticleset;//1
                        XmlDocument monDoc = new XmlDocument();
                        monDoc.LoadXml(resString);
                        var pmcArticleSet = serializer.Deserialize(new StringReader(resString)) as Pmcarticleset;//2

                        //Into db
                        PublicationRepository publicationRepository = new PublicationRepository();
                        List<Publication> lst_publications = ConvertFromPubmedArticleSetToPublications2(pmcArticleSet, disease);
                        if (lst_publications.Count != 0)
                        {
                            publicationRepository.insertListLessFast(lst_publications);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error on disease: " + disease.Name + ", orphaNumber: " + disease.OrphaNumber);
                        //Console.WriteLine("resString: " + resString);
                        Console.WriteLine(e);
                        Console.WriteLine("1: "+GetLine(resString, 105685));
                        Console.WriteLine("2: " + GetLine(resString, 105686));
                        Console.WriteLine("3: " + GetLine(resString, 105687));
                    }
                }

            });
        }
        public static Task DoTheRequestAsync2(List<long> maListe2, Disease disease)
        {
            return new Task(async () =>
            {
                //Initialization
                HttpClient client = new HttpClient();
                List<Publication> results = new List<Publication>();

                //URL and parameters
                string url = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=pmc";
                string ids = String.Join(", ", maListe2.ToArray());
                string jsonInString = JsonConvert.SerializeObject(new
                {
                    //db = "pmc",
                    retstart = $"{0}",
                    retmax = $"{500}",
                    rettype = "xml",
                    retmode = "xml",
                    api_key = $"{ConfigurationManager.Instance.config.API_Key}",
                    email = $"{ConfigurationManager.Instance.config.Email}",
                    tool = $"{ConfigurationManager.Instance.config.Tool}",
                    id = ids
                });

                var parameters = new Dictionary<string, string>{
                    { "db", "pmc" },
                    { "retstart", "0" },
                    { "retmax", "500" },
                    { "rettype", "xml" },
                    { "retmode", "xml" },
                    { "api_key", $"{ConfigurationManager.Instance.config.API_Key}" },
                    { "email", $"{ConfigurationManager.Instance.config.Email}" },
                    { "tool", $"{ConfigurationManager.Instance.config.Tool}" },
                    { "id", ids }
                };

                var urlContent = new FormUrlEncodedContent(parameters);
                Console.WriteLine(urlContent);

                //Make the request and deserialization
                XmlSerializer serializer = new XmlSerializer(typeof(Pmcarticleset));
                using (HttpResponseMessage res = await client.PostAsync(url, urlContent))
                using (HttpContent content = res.Content)
                using (var stream = await content.ReadAsStreamAsync())
                //using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))//1
                {
                    string stringRes = await content.ReadAsStringAsync();//2
                    var reader = new StreamReader(stream, Encoding.UTF8);//2
                    string resString = reader.ReadToEnd();//2
                    //var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });//1
                    try
                    {
                        //var pmcArticleSet = serializer.Deserialize(reader) as Pmcarticleset;//1
                        XmlDocument monDoc = new XmlDocument();
                        monDoc.LoadXml(resString);

                        //Into db
                        PublicationRepository publicationRepository = new PublicationRepository();
                        List<Publication> lst_publications = ConvertFromPubmedArticleSetToPublications3(monDoc, disease);
                        if (lst_publications.Count != 0)
                        {
                            publicationRepository.insertListLessFast(lst_publications);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error on disease: " + disease.Name + ", orphaNumber: " + disease.OrphaNumber);
                        Console.WriteLine(e);
                    }
                }

            });
        }

        public static List<Publication> ConvertFromPubmedArticleSetToPublications3(XmlDocument monDoc, Disease disease)
        {

            XmlNodeList articles = monDoc.GetElementsByTagName("article");

            //Initialisation
            List<Publication> lst_Publications = new List<Publication>();
            for(int i = 0; i < articles.Count; i++)
            {
                lst_Publications.Add(new Publication());
            }

            for (int i = 0; i < articles.Count; i++)
            {
                var articleMeta = articles[i].SelectSingleNode("front/article-meta");

                //Ids
                lst_Publications[i].title = articleMeta?.SelectSingleNode("title-group/article-title/text()")?.Value;
                lst_Publications[i].idPubmed = Convert.ToInt64(articleMeta?.SelectSingleNode("article-id[@pub-id-type='pmid']/text()")?.Value);
                lst_Publications[i].idPMC = Convert.ToInt64(articleMeta?.SelectSingleNode("article-id[@pub-id-type='pmc']/text()")?.Value);
                lst_Publications[i].doi = articleMeta?.SelectSingleNode("article-id[@pub-id-type='doi']/text()")?.Value;


                //Authors
                var contribs = articleMeta?.SelectNodes("contrib-group/contrib/name");
                if(contribs != null)
                {
                    foreach (XmlNode contrib in contribs)
                    {
                        lst_Publications[i].authors.Add(
                        contrib?.SelectSingleNode("given-names/text()")?.Value
                        + " " +
                        contrib?.SelectSingleNode("surname/text()")?.Value);
                    }
                }
                

                //Date
                var dateNode = articleMeta?.SelectSingleNode("pub-date[@pub-type='ppub']");
                if (dateNode != null)
                {
                    int.TryParse(dateNode?.SelectSingleNode("year/text()")?.Value, out int year);
                    int.TryParse(dateNode?.SelectSingleNode("month/text()")?.Value, out int month);
                    int.TryParse(dateNode?.SelectSingleNode("day/text()")?.Value, out int day);
                    if (year != 0 && month != 0 && day != 0)
                    {
                        lst_Publications[i].datePublication = new DateTime(year, month, day);
                    }
                }

                //Times cited
                lst_Publications[i].timesCited = 0;

                //OrphaNumber
                lst_Publications[i].orphaNumberOfLinkedDisease = disease.OrphaNumber;

                
                //Abstract
                lst_Publications[i].abstractText = "";
                var abstracts = articleMeta.SelectNodes("abstract");
                if(abstracts != null)
                {
                    foreach (XmlNode abstractNode in abstracts)
                    {
                        //Selecting direct Ps
                        var Ps = abstractNode?.SelectNodes("p/text()");
                        if(Ps != null)
                        {
                            //Fusioning Ps
                            foreach (XmlNode P in Ps)
                            {
                                lst_Publications[i].abstractText += P?.Value + " ";
                            }
                        }

                        //Selecting indirect Ps
                        var Ps2 = abstractNode?.SelectNodes("sec/p/text()");
                        if (Ps2 != null)
                        {
                            //Fusioning Ps
                            foreach (XmlNode P in Ps2)
                            {
                                lst_Publications[i].abstractText += P?.Value + " ";
                            }
                        }
                    }
                }
                
                
                //FullText
                lst_Publications[i].fullText = "";

                //Selecting direct Ps
                var paragraphes = articles[i]?.SelectNodes("body/sec/p/text()");
                if(paragraphes != null)
                {
                    foreach (XmlNode paragraphe in paragraphes)
                    {
                        lst_Publications[i].fullText += paragraphe?.Value + " ";
                    }
                }

                //Selecting indirect Ps
                var paragraphes2 = articles[i]?.SelectNodes("body/sec/sec/p/text()");
                if (paragraphes2 != null)
                {
                    foreach (XmlNode paragraphe in paragraphes2)
                    {
                        lst_Publications[i].fullText += paragraphe?.Value + " ";
                    }
                }


                Console.WriteLine("i = " + i);
            }


            return lst_Publications;
        }

        public static List<Publication> ConvertFromPubmedArticleSetToPublications2(Pmcarticleset monPmcArticleSet, Disease disease)
        {
            //throw new NotImplementedException();

            List<Publication> lst_Publications = new List<Publication>();
            if (monPmcArticleSet != null && monPmcArticleSet.Article != null)
            {
                foreach (var article in monPmcArticleSet.Article)
                {
                    Publication publication = new Publication();

                    //Title and ids
                    publication.title = article.Front?.Articlemeta?.Titlegroup?.Articletitle?.Text;
                    publication.idPubmed = Convert.ToInt64(article.Front?.Articlemeta?.Articleid?.Where(x => x.Pubidtype == "pmid").FirstOrDefault()?.Text);
                    publication.idPMC = Convert.ToInt64(article.Front?.Articlemeta?.Articleid?.Where(x => x.Pubidtype == "pmc").FirstOrDefault()?.Text);
                    publication.doi = article.Front?.Articlemeta?.Articleid?.Where(x => x.Pubidtype == "doi").FirstOrDefault()?.Text;

                    //Authors
                    foreach (var contribGroup in article.Front?.Articlemeta?.Contribgroup)
                    {
                        var authorsSet = contribGroup.Contrib?.Where(x => x.Contribtype == "author").ToList();
                        if (authorsSet != null)
                        {
                            foreach (var author in authorsSet)
                            {
                                publication.authors.Add(author?.Name?.Givennames + " " + author?.Name?.Surname);
                            }
                        }
                    }
                    

                    //Abstract
                    var abstractE = article.Front?.Articlemeta?.Abstract;

                    if (abstractE != null)
                    {
                        foreach (var part in abstractE)
                        {
                            foreach (var test in part.P)
                            {
                                publication.abstractText += test.Text;
                            }
                        }
                    }

                    //Date
                    var articleDate = article.Front?.Articlemeta?.Pubdate?.Where(x => x.Pubtype == "ppub").FirstOrDefault();
                    if (articleDate != null)
                    {
                        int.TryParse(articleDate?.Year, out int year);
                        int.TryParse(articleDate?.Month, out int month);
                        int.TryParse(articleDate?.Day, out int day);
                        if(year != 0 && month != 0 && day != 0)
                        {
                            publication.datePublication = new DateTime(year, month, day);
                        }
                    }
                    publication.timesCited = 0;

                    publication.orphaNumberOfLinkedDisease = disease.OrphaNumber;

                    lst_Publications.Add(publication);

                    //Full text
                    var sectionFullText = article.Body?.Sec;

                    if (sectionFullText != null)
                    {
                        foreach (var section in sectionFullText)
                        {
                            publication.fullText += section.Title + " ";
                            foreach (var paragraphe in section.P)
                            {
                                publication.fullText += paragraphe.Text + " ";
                            }

                            foreach (var paragraphe in section.sec)
                            {
                                publication.fullText += paragraphe.Title + " ";
                                foreach (var p in paragraphe.P)
                                {
                                    publication.fullText += p.Text + " ";
                                }
                            }
                        }
                    }
                }
            }
            return lst_Publications;
        }

        public static string GetLine(string text, int lineNo)
        {
            string[] lines = text.Replace("\r", "").Split('\n');
            return lines.Length >= lineNo ? lines[lineNo - 1] : null;
        }
    }
}
