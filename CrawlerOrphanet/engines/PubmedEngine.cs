using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using MongoRepository.entities;
using System.Threading;
using System.Text;
using CrawlerOrphanet.tools;
using WebCrawler2;

namespace WebCrawler
{
    public class PubmedEngine
    {
        public ConcurrentBag<Publication> Publications { get; set; }

        public string confAPI { get; set; }

        private MongoRepository.PublicationRepository publicationRepository;

        public PubmedEngine()
        {
            publicationRepository = new MongoRepository.PublicationRepository();
            Publications = new ConcurrentBag<Publication>();
            confAPI = "&api_key=5ed9a0500dd91e18d27009f9e7160d7a7008&tool=RDSearch&email=charles.cousyn1@uqac.ca";
            //myRateLimiter.LaunchRequests();
        }

        public eSearchResult PubmedCrawlerSearch(Disease disease)
        {
            var term = disease.Name + " AND hasabstract[text] AND Humans[Mesh]";
            var url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=pubmed&term={term}&sort=relevance&retmax=1000&usehistory=y{confAPI}";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            request.AutomaticDecompression = DecompressionMethods.GZip;

            //Console.WriteLine("Starting request for PubMed ESearch this can take some time...");

            XmlSerializer serializer = new XmlSerializer(typeof(eSearchResult));
            eSearchResult result = new eSearchResult();

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
                {
                    //Console.WriteLine("Parsing request for PubMed ESearch this can take some time...");
                    result = serializer.Deserialize(reader) as eSearchResult;
                }
            }
            catch
            {

            }

            return result;
        }

        //Recursive function
        //public Task PubmedCrawlerFetch(string webDev, long retstart, int queryKey, int maxBatch, Disease disease)
        public Task PubmedCrawlerFetch(long retstart, int maxBatch, Disease disease, List<long> idList)
        {
            return new Task(() =>
            {
                //var url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=pubmed&WebEnv={webDev}&query_key={queryKey}&retstart={retstart}&retmax={maxBatch}&rettype=xml&retmode=xml{confAPI}";
                var url = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi";

                //Constructing parameters ids
                string parameters = $"db=pubmed&retstart={retstart}&retmax={maxBatch}&rettype=xml&retmode=xml{confAPI}&id=" + idList.ElementAt(0).ToString();
                //string parameters = $"db=pubmed&retstart={retstart}&retmax={maxBatch}{confAPI}&id=" + idList.ElementAt(0).ToString();
                for(int i = 1; i< idList.Count; i++)
                {
                    parameters += ","+ idList.ElementAt(i);
                }

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method ="POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
                request.AutomaticDecompression = DecompressionMethods.GZip;


                var data = Encoding.ASCII.GetBytes(parameters);
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                //Console.WriteLine("Starting request for PubMed Fetch this can take some time...");

                XmlSerializer serializer = new XmlSerializer(typeof(PubmedArticleSet));
                PubmedArticleSet results;
                
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var readerString = new StreamReader(stream, Encoding.UTF8))
                //using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
                {

                    string resString = readerString.ReadToEnd();
                    try
                    {
                        //Console.WriteLine("Parsing request for PubMed Fetch this can take some time...");
                        results = serializer.Deserialize(new StringReader(resString)) as PubmedArticleSet;
                        //results = serializer.Deserialize(reader) as PubmedArticleSet;
                        //Into db
                        List<Publication> lst_publications = ConvertFromPubmedArticleSetToPublications(results, disease);
                        if (lst_publications.Count != 0)
                        {
                            publicationRepository.insertListLessFast(lst_publications);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error on disease: " + disease.Name + ", orphaNumber: " + disease.OrphaNumber);
                        Console.WriteLine(resString);
                    }
                }
            }
            );
        }

        //Recursive function
        //public Task PubmedCrawlerFetch(string webDev, long retstart, int queryKey, int maxBatch, Disease disease)
        public Task PubmedCrawlerFetch2(long retstart, int maxBatch, Disease disease, List<long> idList)
        {
            return new Task(() =>
            {
                //var url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=pmc&WebEnv={webDev}&query_key={queryKey}&retstart={retstart}&retmax={maxBatch}&rettype=xml&retmode=xml{confAPI}";
                var url = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi";

                //Constructing parameters ids
                string parameters = $"db=pmc&retstart={retstart}&retmax={maxBatch}&rettype=xml&retmode=xml{confAPI}&id=" + idList.ElementAt(0).ToString();
                //string parameters = $"db=pubmed&retstart={retstart}&retmax={maxBatch}{confAPI}&id=" + idList.ElementAt(0).ToString();
                for (int i = 1; i < idList.Count; i++)
                {
                    parameters += "," + idList.ElementAt(i);
                }

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
                request.AutomaticDecompression = DecompressionMethods.GZip;


                var data = Encoding.ASCII.GetBytes(parameters);
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                //Console.WriteLine("Starting request for PubMed Fetch this can take some time...");

                XmlSerializer serializer = new XmlSerializer(typeof(Pmcarticleset));
                Pmcarticleset results;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var readerString = new StreamReader(stream, Encoding.UTF8))
                //using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
                {

                    string resString = readerString.ReadToEnd();
                    try
                    {
                        //Console.WriteLine("Parsing request for PubMed Fetch this can take some time...");
                        results = serializer.Deserialize(new StringReader(resString)) as Pmcarticleset;
                        //results = serializer.Deserialize(reader) as PubmedArticleSet;
                        //Into db
                        List<Publication> lst_publications = ConvertFromPubmedArticleSetToPublications2(results, disease);
                        if (lst_publications.Count != 0)
                        {
                            publicationRepository.insertListLessFast(lst_publications);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error on disease: " + disease.Name + ", orphaNumber: " + disease.OrphaNumber);
                        Console.WriteLine(resString);
                    }
                }
            }
            );
        }

        public Task PubmedCrawlerLink(long retstart, int maxBatch,  List<long> idList)
        {
            return new Task(() =>
                {
                    if(idList == null || idList.Count == 0)
                    {
                        return;
                    }
                    var parameters = $"dbfrom=pubmed&linkname=pubmed_pubmed_citedin" +
                    $"&retstart={retstart}&retmax={maxBatch}&rettype=xml&retmode=xml{confAPI}&id={+idList.ElementAt(0)}";

                    for (int i = 1; i < idList.Count; i++)
                    {
                        parameters += "&id=" + idList.ElementAt(i);
                    }

                    var url = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/elink.fcgi";
                    //Console.WriteLine(url);

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;


                    var data = Encoding.ASCII.GetBytes(parameters);
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    
                    //Console.WriteLine("Starting request for PubMed citations this can take some time...");

                    XmlSerializer serializer = new XmlSerializer(typeof(ELinkResult));
                    ELinkResult results;

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
                    {
                        //Console.WriteLine("Parsing request for PubMed this can take some time...");
                        results = serializer.Deserialize(reader) as ELinkResult;

                        foreach(LinkSet linkset in results.LinkSet)
                        {
                            if(linkset.LinkSetDb != null)
                            {
                                Publication pub = publicationRepository.getByIDPubmed(linkset.IdList.Id);
                                pub.timesCited = linkset.LinkSetDb.Link.Count;
                                publicationRepository.updatePublication(pub);
                            }
                            
                        }
                    }
                }
            );
                
        }

        private List<Publication> ConvertFromPubmedArticleSetToPublications(PubmedArticleSet pubmedArticleSet, Disease disease)
        {
            List<Publication> lst_Publications = new List<Publication>();
            if(pubmedArticleSet != null && pubmedArticleSet.PubmedArticle != null)
            {
                foreach (var article in pubmedArticleSet.PubmedArticle)
                {
                    Publication publication = new Publication();
                    publication.title = article.MedlineCitation.Article.ArticleTitle;
                    publication.idPubmed = article.MedlineCitation.PMID.Value;

                    if (article.MedlineCitation.Article.AuthorList != null)
                    {
                        foreach (var author in article.MedlineCitation.Article.AuthorList.Author)
                        {
                            publication.authors.Add(author?.ForeName + " " + author?.LastName);
                        }
                    }


                    var abstractE = article.MedlineCitation.Article.Abstract;

                    if (abstractE != null)
                    {
                        foreach (var section in abstractE.AbstractText)
                        {
                            publication.abstractText += section?.Label + " " + section?.Value + " ";
                        }
                    }


                    if (article.MedlineCitation.DateCreated != null)
                    {
                        publication.datePublication = new DateTime(article.MedlineCitation.DateCreated.Year, article.MedlineCitation.DateCreated.Month, article.MedlineCitation.DateCreated.Day);
                    }
                    publication.timesCited = 0;

                    publication.orphaNumberOfLinkedDisease = disease.OrphaNumber;

                    lst_Publications.Add(publication);
                }
            }
            return lst_Publications;
        }

        private List<Publication> ConvertFromPubmedArticleSetToPublications2(Pmcarticleset pmcArticleSet, Disease disease)
        {
            throw new NotImplementedException();
            /*
            List<Publication> lst_Publications = new List<Publication>();
            if (pubmedArticleSet != null && pubmedArticleSet.PubmedArticle != null)
            {
                foreach (var article in pubmedArticleSet.PubmedArticle)
                {
                    Publication publication = new Publication();
                    publication.title = article.MedlineCitation.Article.ArticleTitle;
                    publication.idPubmed = article.MedlineCitation.PMID.Value;

                    if (article.MedlineCitation.Article.AuthorList != null)
                    {
                        foreach (var author in article.MedlineCitation.Article.AuthorList.Author)
                        {
                            publication.authors.Add(author?.ForeName + " " + author?.LastName);
                        }
                    }


                    var abstractE = article.MedlineCitation.Article.Abstract;

                    if (abstractE != null)
                    {
                        foreach (var section in abstractE.AbstractText)
                        {
                            publication.abstractText += section?.Label + " " + section?.Value + " ";
                        }
                    }


                    if (article.MedlineCitation.DateCreated != null)
                    {
                        publication.datePublication = new DateTime(article.MedlineCitation.DateCreated.Year, article.MedlineCitation.DateCreated.Month, article.MedlineCitation.DateCreated.Day);
                    }
                    publication.timesCited = 0;

                    publication.orphaNumberOfLinkedDisease = disease.OrphaNumber;

                    lst_Publications.Add(publication);
                }
            }
            return lst_Publications;*/
        }

        public void Start(List<Disease> lst_diseases)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            

            Console.WriteLine("ESearch...");
            List<Task> efetchsToDo = new List<Task>();
            int compteur = 0;
            Parallel.ForEach(lst_diseases, (disease) =>
                {
                    //We delete all publication linked to this disease
                    publicationRepository.removeFromOrphaNumber(disease.OrphaNumber);

                    eSearchResult res = this.PubmedCrawlerSearch(disease);
                
                    if (res.IdList != null && res.Count != 0)
                    {

                        List<long> idListTemp = res.IdList.ToList();

                        //Batch and size
                        int batchSizeTemp = 500;
                        int nombreBatchTemp = (res.IdList.Length / batchSizeTemp) + 1;
                        if((nombreBatchTemp - 1)* batchSizeTemp == res.IdList.Length)
                        {
                            nombreBatchTemp--;
                        }

                        for (int i = 0; i < nombreBatchTemp; i++)
                        {
                            //efetchsToDo.Add(this.PubmedCrawlerFetch(res.WebEnv, i * batchSize, res.QueryKey, batchSize, disease));
                            efetchsToDo.Add(this.PubmedCrawlerFetch(i * batchSizeTemp, batchSizeTemp, disease, idListTemp));
                        }
                    }
                    ProgressBar.Instance.Report((double)compteur / lst_diseases.Count);
                    compteur++;
                }
            );
            ProgressBar.Instance.Dispose();

            Thread.Sleep(100);

            //ProgressBar.Instance.ResetCompteur();
            //ProgressBar.Instance.operationToDo = efetchsToDo.Count;
            TimeLeft.Instance.operationsToDo = efetchsToDo.Count;
            Console.WriteLine(efetchsToDo.Count+" Efetchs to do...");
            compteur = 0;
            for(int i =0; i < efetchsToDo.Count; i+=10)
            {
                int number = 10;
                if((i+10) > efetchsToDo.Count)
                {
                    number = efetchsToDo.Count - i;
                }
                var selectedEfetchs = efetchsToDo.GetRange(i, number);
                Stopwatch diffTime = new Stopwatch();
                diffTime.Start();

                Parallel.ForEach(selectedEfetchs, (efetch) =>
                    {
                        efetch.Start();
                        /*efetch.Wait();
                        progress.Report((double)compteur / efetchsToDo.Count);
                        compteur++;*/
                    }
                );
                Task.WaitAll(selectedEfetchs.ToArray());

                diffTime.Stop();
                TimeLeft.Instance.IncrementOfXOperations(TimeSpan.FromMilliseconds(diffTime.ElapsedMilliseconds).Seconds, number);
                TimeLeft.Instance.CalcAndShowTimeLeft();

                if(diffTime.ElapsedMilliseconds < 1000)
                {
                    Thread.Sleep((int)TimeSpan.FromSeconds(1).TotalMilliseconds - (int)diffTime.ElapsedMilliseconds + 10);
                }
            }

            TimeLeft.Instance.Reset();

            //Elinks to do

            List<Task> elinksToDo = new List<Task>();
            List<long> idList = publicationRepository.selectAll().Select(x=>x.idPubmed).ToList();
            //Batch and size
            int batchSize = 500;
            int nombreBatch = (idList.Count / batchSize) + 1;
            if ((nombreBatch - 1) * batchSize == idList.Count)
            {
                nombreBatch--;
            }

            for (int i = 0; i < nombreBatch; i++)
            {
                elinksToDo.Add(this.PubmedCrawlerLink(i * batchSize, batchSize, idList));
            }


            TimeLeft.Instance.operationsToDo = elinksToDo.Count;
            Console.WriteLine(elinksToDo.Count + " Elinks to do...");
            compteur = 0;
            for (int i = 0; i < elinksToDo.Count; i += 10)
            {
                int number = 10;
                if ((i + 10) > elinksToDo.Count)
                {
                    number = elinksToDo.Count - i;
                }
                var selectedElinks = elinksToDo.GetRange(i, number);
                Stopwatch diffTime = new Stopwatch();
                diffTime.Start();

                Parallel.ForEach(selectedElinks, (elink) =>
                    {
                        elink.Start();
                    }
                );
                Task.WaitAll(selectedElinks.ToArray());

                diffTime.Stop();
                
                TimeLeft.Instance.IncrementOfXOperations(TimeSpan.FromMilliseconds(diffTime.ElapsedMilliseconds).Seconds, number);
                TimeLeft.Instance.CalcAndShowTimeLeft();

                if (diffTime.ElapsedMilliseconds < 1000)
                {
                    Thread.Sleep((int)TimeSpan.FromSeconds(1).TotalMilliseconds - (int)diffTime.ElapsedMilliseconds + 10);
                }
            }


            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds);
            Console.WriteLine("RunTime " + elapsedTime);
            //myRateLimiter.Stop();
        }
    }
}
