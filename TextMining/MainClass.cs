using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.aliasi.chunk;
using com.aliasi.dict;
using com.aliasi.spell;
using com.aliasi.tokenizer;
using java.lang;
using java.util;

namespace TextMining
{
    public class MainClass
    {
        public static void Main(string[] args)
        {

            List<System.String> phenotypes = new List<System.String>();
            TrieDictionary dict = new TrieDictionary();
            
            foreach (System.String pheno in phenotypes)
            {
                DictionaryEntry entry = new DictionaryEntry(pheno, "PHENOTYPE");
                dict.addEntry(entry);
            }

            TokenizerFactory tokenizerFactory = IndoEuropeanTokenizerFactory.INSTANCE;
            WeightedEditDistance editDistance = new FixedWeightEditDistance(0, -1, -1, -1, System.Double.NaN);

            double maxDistance = 0.0;
            ApproxDictionaryChunker chunker = new ApproxDictionaryChunker(dict, tokenizerFactory, editDistance, maxDistance);

            //Use STDIN JSON
            List<System.String> texts = new List<System.String>();

            foreach (string text in texts)
            {
                //Text preprocessing
                System.String newText = text.ToLower();

                Chunking chunking = chunker.chunk(newText);
                CharSequence cs = chunking.charSequence();
                Set chunkSet = chunking.chunkSet();
                Chunk[] tab = (Chunk[]) chunkSet.toArray();
                for(int i = 0; i < tab.Length; i++)
                {
                    int start = tab[i].start();
                    int end = tab[i].end();
                    CharSequence str = cs.subSequence(start, end);
                    //double distance = chunk.score();
                    //string match = chunk.type();

                    //System.out.print(str);
                }

            }
        }

        
    }
}
