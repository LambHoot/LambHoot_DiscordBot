using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lambhootDiscordBot
{
    class PartialBiGraph
    {
        public Dictionary<string, Word> vocabulary;
        private System.IO.StreamReader file;
        private string trainingFilePath;

        public PartialBiGraph()
        {
            vocabulary = new Dictionary<string, Word>();
            SetUp();
        }

        public void SetUp()
        {
            Console.WriteLine("Training filepath: ");
            trainingFilePath = @"" + Console.ReadLine();
            file = new System.IO.StreamReader(trainingFilePath);
            buildVocabulary(file);
            var x = vocabulary;
        }

        #region TextFile and sentence parsing

        public void buildVocabulary(System.IO.StreamReader reader)
        {
            string readLine;
            while ((readLine = reader.ReadLine()) != null)
            {
                addWordsFromLine(readLine);
            }
            //update probs
            processProbabilities();
        }

        public void addWordsFromLine(string line)
        {
            string[] stringWords = line.Split(' ');
            for(int i = 0; i < stringWords.Count(); i++)
            {
                if (vocabulary.ContainsKey(stringWords[i]))
                {
                    //if the word is already there, increase it's count
                    vocabulary[stringWords[i]].addWordCount();
                    
                }
                else
                {
                    //otherwise add it to the vocab
                    vocabulary.Add(stringWords[i], new Word(stringWords[i]));
                }
                //add the wordAfter if possible
                if (i + 1 < stringWords.Count())
                    vocabulary[stringWords[i]].addWordAfter(stringWords[i + 1]);
            }
        }

        //to be called once the vocabulary is full (also handles words' wordAfter probs)
        public void processProbabilities()
        {
            foreach (Word w in vocabulary.Values)
            {
                w.wordProb = (float)w.getWordCount() / vocabulary.Count();
                w.processWordAfterProbabilities();
            }
        }

        #endregion TextFile and sentence parsing


    }



    #region Word Class

    public class Word
    {
        public string wordString;
        private int wordCount;//the number of instances of this word in the training set, its importance in the vocabulary
        public float wordProb;//the count of this word / total vocabulary size
        public List<string> wordAfterList;//list of all words appearing immediately after this word
        public List<int> wordAfterCountList;//counts for the number of instances of the words in wordAfterList
        public List<float> wordAfterProbList;//probabilities for each of the words in wordAfterList (value between 0/1)

        public Word(string wordString)
        {
            this.wordString = wordString;
            wordCount = 1;
            wordAfterList = new List<string>();
            wordAfterCountList = new List<int>();
            wordAfterProbList = new List<float>();
        }

        public void addWordCount()
        {
            wordCount++;
        }

        public int getWordCount()
        {
            return wordCount;
        }

        public void addWordAfter(string wordAfter)
        {
            int index = wordAfterList.IndexOf(wordAfter);
            if (index != -1)//wordAfter already stored
            {
                wordAfterCountList[index] += 1;//increase the count
            }
            else//wordAfter is new
            {
                wordAfterList.Add(wordAfter);
                wordAfterCountList.Add(1);
            }
        }

        //is called when all wordAfters are added and the wordAfterProbList is still empty
        public void processWordAfterProbabilities()
        {
            float totalWordAfterCount = wordAfterCountList.Sum();
            foreach (int count in wordAfterCountList)
                wordAfterProbList.Add((float)count/totalWordAfterCount);
        }


        //THE IMPORTANT HEURISTIC METHOD
        public string nextChosenWord()
        {
            //returns a word of possible
            //otherwise returns null
            if (wordAfterList.Count() == 0)
                return null;
            float prob = randomProbability();
            //pick the wordAfter with probability greater and closest to prob
            int currentBestIndex = 0;
            for(int j = 0; j < wordAfterProbList.Count(); j++)
            {
                //TO COMPLETE
            }



            return "";
        }


        public override string ToString()
        {
            return wordString;
        }

        public static float randomProbability()
        {
            float prob = (float)MyBot.randomDoubleRange(0.0, 100.0);
            prob /= 100f;//I hate myself
            return prob;
        }

    }

    #endregion Word Class

}
