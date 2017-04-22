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

        public List<string> wordAfterList;//list of all words appearing immediately after this word
        public List<string> wordAfterList2;//for words two spaces away in a sentence

        public List<int> wordAfterCountList;//counts for the number of instances of the words in wordAfterList
        public List<int> wordAfterCountList2;

        public List<float> wordAfterProbList;//probabilities for each of the words in wordAfterList (value between 0/1)
        public List<float> wordAfterProbList2;

        private float minWordAfterProb = 99, maxWordAfterProb = int.MinValue;
        private float minWordAfterProb2 = 99, maxWordAfterProb2 = int.MinValue;

        #region Word stuff

        public Word(string wordString)
        {
            this.wordString = wordString;
            wordCount = 1;
            wordAfterList = new List<string>();
            wordAfterCountList = new List<int>();
            wordAfterProbList = new List<float>();

            wordAfterList2 = new List<string>();
            wordAfterCountList2 = new List<int>();
            wordAfterProbList2 = new List<float>();
        }

        public void addWordCount()
        {
            wordCount++;
        }

        public int getWordCount()
        {
            return wordCount;
        }

        public void addWordAfter(string wordAfter, int gramNumber = 1)
        {
            //as to not have to pass multiple references to lists in this otherwise simple method,
            //I'm going to copy the same behavior twice in an if/else
            if (gramNumber == 1) {
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
            else if (gramNumber == 2)
            {
                int index = wordAfterList2.IndexOf(wordAfter);
                if (index != -1)//wordAfter already stored
                {
                    wordAfterCountList2[index] += 1;//increase the count
                }
                else//wordAfter is new
                {
                    wordAfterList2.Add(wordAfter);
                    wordAfterCountList2.Add(1);
                }
            }
        }

        //is called when all wordAfters are added and the wordAfterProbList is still empty
        public void processWordAfterProbabilities()
        {
            float totalWordAfterCount = wordAfterCountList.Sum();
            foreach (int count in wordAfterCountList)
            {
                wordAfterProbList.Add((float)count / totalWordAfterCount);
                float newProb = wordAfterProbList.Last();
                maxWordAfterProb = newProb > maxWordAfterProb ? newProb : maxWordAfterProb;
                minWordAfterProb = newProb < minWordAfterProb ? newProb : minWordAfterProb;
            }

            float totalWordAfterCount2 = wordAfterCountList2.Sum();
            foreach (int count in wordAfterCountList2)
            {
                wordAfterProbList2.Add((float)count / totalWordAfterCount2);
                float newProb = wordAfterProbList2.Last();
                maxWordAfterProb2 = newProb > maxWordAfterProb2 ? newProb : maxWordAfterProb2;
                minWordAfterProb2 = newProb < minWordAfterProb2 ? newProb : minWordAfterProb2;
            }
            //Note: not use list.Max() or .Min() here because this also handles words with 0 wordsAfter
        }

        #endregion Word stuff


        //THE IMPORTANT HEURISTIC METHOD (for random sentences only)
        public string nextChosenWord()
        {
            //returns a word of possible
            //otherwise returns null
            string returnString = null;
            int maxLoopAttempts = 7;

            if (wordAfterList.Count() == 0)
                return returnString;

            if (wordAfterList.Count() == 1 && (MyBot.randomDoubleRange(0, 100) > 50))
            {
                return returnString;
            }

            int currentBestIndex = -1;
            float currentSmallestDifference = 99;

            for (int i = 0; i < maxLoopAttempts; i++)
            {
                //pick the wordAfter with probability greater and closest to prob
                float prob = randomProbabilityForWord();
                var x = this;
                for (int j = 0; j < wordAfterProbList.Count(); j++)
                {
                    float thisDifference = wordAfterProbList[j] - prob;
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
                    returnString = wordAfterList[currentBestIndex];
                    break;
                }
            }
            return returnString;
        }


        //USING FULL PARTIAL BIGRAPH AND BAYESIAN PROBABILITY
        public float probabilityGivenSentence(List<Word> currentSentence)
        {
            //returns a probability
            float nextWordProb = 0;

            //special case, first word or second word
            if (!(currentSentence.Count() < 1))
            {
                //calculate conditional probabilities of all words in vocab given these two words
                //pick the word with the best probability
                Word wordB = currentSentence[currentSentence.Count() - 1];
                if (currentSentence.Count() == 1)
                {
                    //RETURNS INFINITY!
                    float x = this.ProbabilityOfWordgivenB(wordB);
                    double y = Math.Log(x);
                    float z = (float)Math.Abs(y);

                    nextWordProb = myLog(this.ProbabilityOfWordgivenB(wordB));
                }
                else if(currentSentence.Count() > 1) {
                    Word wordBB = currentSentence[currentSentence.Count() - 2];
                    nextWordProb = myLog(this.ProbabilityOfWordgivenB(wordB)) + myLog(wordB.ProbabilityOfWordgivenB(wordBB));
                }
            }
            //RETURNS INFINITY!
            return nextWordProb;
        }


        private float getProbabilityOfWordAfter(string word, int gramNumber = 1)
        {
            float wordAfterProb = 0;
            if(gramNumber == 1)
            {
                if (wordAfterList.Contains(word))
                {
                    for(int i = 0; i < wordAfterList.Count(); i++)
                    {
                        wordAfterProb = (wordAfterList[i].Equals(word)) ? wordAfterProbList[i] : wordAfterProb;
                    }
                }
            }
            else if(gramNumber == 2)
            {
                if (wordAfterList2.Contains(word))
                {
                    for (int i = 0; i < wordAfterList2.Count(); i++)
                    {
                        wordAfterProb = (wordAfterList2[i].Equals(word)) ? wordAfterProbList2[i] : wordAfterProb;
                    }
                }
            }
            return wordAfterProb;
        }

        #region UTILS

        public override string ToString()
        {
            return wordString;
        }

        public float randomProbabilityForWord()
        {
            float prob = (float)MyBot.randomDoubleRange(minWordAfterProb * 0.8, maxWordAfterProb * 1.2);
            // 0.8 and 1.2 here because we want an even distribution of chance for the case that a word has only 1 wordAfter
            return prob;
        }

        public float ProbabilityOfWordgivenB(Word b)
        {
            float numerator = (b.getProbabilityOfWordAfter(wordString) * wordProb);
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

}
