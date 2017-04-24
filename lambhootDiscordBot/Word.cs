using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lambhootDiscordBot
{
    public class Word
    {
        public string wordString;
        private int wordCount;//the number of instances of this word in the training set, its importance in the vocabulary
        public float wordProb;//the count of this word / total vocabulary size

        //public List<string> wordAfterList;//list of all words appearing immediately after this word
        //public List<string> wordAfterList2;//for words two spaces away in a sentence

        //public List<int> wordAfterCountList;//counts for the number of instances of the words in wordAfterList
        //public List<int> wordAfterCountList2;

        //public List<float> wordAfterProbList;//probabilities for each of the words in wordAfterList (value between 0/1)
        //public List<float> wordAfterProbList2;

        public List<List<WordAfter>> wordAfterLists;
        //public List<List<int>> wordAfterCountLists;
        //public List<List<float>> wordAfterProbLists;

        private List<float> minWordAfterProbList;
        public List<float> maxWordAfterProbList;

        //private float minWordAfterProb = 99, maxWordAfterProb = int.MinValue;
        //private float minWordAfterProb2 = 99, maxWordAfterProb2 = int.MinValue;

        #region Word stuff

        public Word(string wordString)
        {
            this.wordString = wordString;
            wordCount = 1;
            //wordAfterList = new List<string>();
            //wordAfterCountList = new List<int>();
            //wordAfterProbList = new List<float>();

            //wordAfterList2 = new List<string>();
            //wordAfterCountList2 = new List<int>();
            //wordAfterProbList2 = new List<float>();

            wordAfterLists = new List<List<WordAfter>>();

            minWordAfterProbList = new List<float>();
            maxWordAfterProbList = new List<float>();
        }

        public void addWordCount()
        {
            wordCount++;
        }

        public int getWordCount()
        {
            return wordCount;
        }

        public void addWordAfter(string wordAfter, int gramNumber = 0)
        {
            int index = -1;
            if (wordAfterLists.Count() - 1 >= gramNumber)//if the gram exists yet
                index = wordAfterLists[gramNumber].FindIndex((WordAfter w) => { return w.wordString == wordAfter; });
            if (index != -1)//wordAfter already stored
            {
                wordAfterLists[gramNumber][index].count++;//increase the count
            }
            else//wordAfter is new
            {
                if (wordAfterLists.Count() - 1 < gramNumber)
                    wordAfterLists.Add(new List<WordAfter>());
                wordAfterLists[gramNumber].Add(new WordAfter(wordAfter, gramNumber));
            }
        }

        //is called when all wordAfters are added and the wordAfterProbList is still empty
        public void processWordAfterProbabilities()
        {
            foreach (List<WordAfter> waList in wordAfterLists)
            {
                minWordAfterProbList.Add(float.MaxValue);
                maxWordAfterProbList.Add(float.MinValue);
                int totalWordAfterCount = 0;
                foreach (WordAfter wa in waList)
                    totalWordAfterCount += wa.count;
                foreach (WordAfter wa in waList)
                {
                    float newProb = wa.updateProb(totalWordAfterCount);
                    if (newProb < minWordAfterProbList.Last())
                        minWordAfterProbList[minWordAfterProbList.Count()-1] = newProb;
                    if (newProb > maxWordAfterProbList.Last())
                        maxWordAfterProbList[maxWordAfterProbList.Count()-1] = newProb;
                }
                //this is less optimal now, possibly, depending on how List.Sum() works
            }
        }

        #endregion Word stuff


        //THE IMPORTANT HEURISTIC METHOD (for random sentences only)
        public string nextChosenWord(int gn = 0)
        {
            //returns a word of possible
            //otherwise returns null
            string returnString = null;
            int maxLoopAttempts = 7;

            if (wordAfterLists[gn].Count() == 0)
                return returnString;

            if (wordAfterLists[gn].Count() == 1 && (MyBot.randomDoubleRange(0, 100) > 50))
            {
                return returnString;
            }

            int currentBestIndex = -1;
            float currentSmallestDifference = 99;

            for (int i = 0; i < maxLoopAttempts; i++)
            {
                //pick the wordAfter with probability greater and closest to prob
                float prob = randomProbabilityForWord(gn);
                var x = this;
                for (int j = 0; j < wordAfterLists[gn].Count(); j++)
                {
                    float thisDifference = wordAfterLists[gn][j].prob - prob;
                    if (thisDifference < 0)//if lower, ignore
                        continue;
                    if (thisDifference < currentSmallestDifference)
                    {
                        currentSmallestDifference = thisDifference;
                        currentBestIndex = j;
                    }
                }
                if (currentBestIndex != -1)
                {
                    returnString = wordAfterLists[gn][currentBestIndex].wordString;
                    break;
                }
            }
            return returnString;
        }


        //USING FULL PARTIAL NGRAM AND BAYESIAN PROBABILITY
        public float probabilityGivenSentence(List<Word> currentSentence)
        {
            //returns a probability
            float nextWordProb = 0;
            List<float> probsList = new List<float>();

            //add all conditional probs to a list
            for(int i = 0; i < currentSentence.Count(); i++)
            {
                float probForThisWord = this.ProbabilityOfWordgivenB(currentSentence[i], currentSentence.Count() - (i+1));
                probsList.Add(probForThisWord);
                for(int j = i+1; j+1 < currentSentence.Count(); j++)
                {
                    Word word1 = currentSentence[i];
                    Word word2 = currentSentence[j];
                    float probForSentenceWords = word2.ProbabilityOfWordgivenB(word1, j-(i+1));
                    probsList.Add(probForSentenceWords);
                }
            }

            //sum logs of all probs
            foreach (float p in probsList)
            {
                nextWordProb += myLog(p);
            }
            return nextWordProb += this.wordProb;
        }


        private float getProbabilityOfWordAfter(string word, int gn = 0)
        {
            float wordAfterProb = 0;
            if (wordAfterLists.Count() > gn)
            {
                int index = wordAfterLists[gn].FindIndex((WordAfter w) => { return w.wordString == word; });
                if (index != -1)
                {
                    wordAfterProb = wordAfterLists[gn][index].prob;
                }
            }
            return wordAfterProb;
        }

        #region UTILS

        public override string ToString()
        {
            return wordString;
        }

        public float randomProbabilityForWord(int gn = 0)
        {
            float prob = (float)MyBot.randomDoubleRange(minWordAfterProbList[gn] * 0.8, maxWordAfterProbList[gn] * 1.2);
            // 0.8 and 1.2 here because we want an even distribution of chance for the case that a word has only 1 wordAfter
            return prob;
        }

        public float ProbabilityOfWordgivenB(Word b, int gn = 0)
        {
            float numerator = (b.getProbabilityOfWordAfter(wordString, gn) * wordProb);
            if (numerator == 0f || wordString.Equals(b.wordString))//if they are independent variables or they are the same, probability is done differently
                return 0f;
            float prob = numerator / b.wordProb;
            //Note: +1 to avoid pure 0 probabilities
            return prob;
        }

        public static float myLog(float num)
        {
            if (num == 0f)
                return 0f;
            else
                return (float)Math.Abs(Math.Log(num));
        }

        #endregion UTILS

    }


    public class WordAfter : IComparable<string> {//it's about time!
        public string wordString;
        public int orderAfter;//an int to represent the number of spaces ahead the word is in the sentence
        public int count;
        public float prob;

        public WordAfter(string ws, int order)
        {
            wordString = ws;
            orderAfter = order;
            count = 1;
            prob = 0;//will be updated later
        }

        public float updateProb(int totalWordAfterCount)
        {
            prob = (float)count / totalWordAfterCount;
            return prob;
        }

        public int CompareTo(string otherString)//used for searching in lists
        {
            return this.wordString.CompareTo(otherString);
        }
    }

}
