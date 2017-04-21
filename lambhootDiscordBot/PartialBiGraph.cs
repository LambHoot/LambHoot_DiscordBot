using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lambhootDiscordBot
{
    class PartialBiGram
    {
        public Dictionary<string, Word> vocabulary;
        private System.IO.StreamReader file;
        private string trainingFilePath;
        public static int minSentenceLength = 1, maxSentenceLength = int.MinValue;
        public static float minVocabWordProb = 99, maxVocabWordProb = int.MinValue;

        public PartialBiGram()
        {
            vocabulary = new Dictionary<string, Word>();
            SetUp();
        }

        public PartialBiGram(string filePath)
        {
            vocabulary = new Dictionary<string, Word>();
            SetUp(filePath);
        }

        public void SetUp()
        {
            Console.WriteLine("Training filepath: ");
            trainingFilePath = @"" + Console.ReadLine();
            file = new System.IO.StreamReader(trainingFilePath);
            buildVocabulary(file);
            file.Close();
        }

        public void SetUp(string filePath)
        {
            trainingFilePath = @"" + filePath;
            file = new System.IO.StreamReader(trainingFilePath);
            buildVocabulary(file);
            file.Close();
        }

        public void retrain()
        {
            vocabulary = new Dictionary<string, Word>();//RESET THE VOCABULARY DUH!
            minSentenceLength = 1;
            maxSentenceLength = int.MinValue;//AND RESET MAX AND MIN LENGTHS
            file = new System.IO.StreamReader(trainingFilePath);
            buildVocabulary(file);
            file.Close();
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
            if (String.IsNullOrWhiteSpace(line))//occured when users entered the Bee Movie Script
                return;
            string[] stringWords = line.Split(' ');
            //updates maxSentenceLength
            maxSentenceLength = stringWords.Count() > maxSentenceLength ? stringWords.Count() : maxSentenceLength;
            for(int i = 0; i < stringWords.Count(); i++)
            {
                if (String.IsNullOrWhiteSpace(stringWords[i]))//occured when users entered the Bee Movie Script
                    return;//being extra cautious

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
                maxVocabWordProb = w.wordProb > maxVocabWordProb ? w.wordProb : maxVocabWordProb;
                minVocabWordProb = w.wordProb < minVocabWordProb ? w.wordProb : minVocabWordProb;

                //for word's own probabilities
                w.processWordAfterProbabilities();
            }
        }

        #endregion TextFile and sentence parsing


        #region Sentence Generation

        //GENERATE NEW SENTENCE
        public string generateNewSentence(string input = null)
        {
            //build list of words
            List<Word> sentence = new List<Word>();
            string returnString = "";
            int sentenceLength = (int)MyBot.randomDoubleRange(minSentenceLength, maxSentenceLength*0.6);

            if (input == null)
            {
                sentence.Add(selectRandomWord());//start random generation
                returnString += " " + sentence.Last();
            }
            else//handle starting sentence from input
            {
                string[] inputArray = input.Split(' ');
                string inputWord = inputArray.Last();
                if (!vocabulary.ContainsKey(inputWord))
                    sentence.Add(selectRandomWord());//if not in vocab, just start random sentence
                else
                {
                    for (int i = 0; i < inputArray.Count() - 1; i++)
                        returnString += " " + inputArray[i];
                    Word inputStartingWord = vocabulary[inputWord];
                    sentence.Add(inputStartingWord);
                    returnString += " " + sentence.Last();
                }

            }

            while (sentence.Count() < sentenceLength)
            {
                string newWordKey = sentence.Last().nextChosenWord();
                if (newWordKey != null)
                {
                    sentence.Add(vocabulary[newWordKey]);
                    returnString += " " + sentence.Last();
                }
                else
                {
                    if (sentence.Count() > 0)//possibility that sentence has failed on first try, in which case, don't do this
                    {
                        if (!Char.IsPunctuation(returnString.Last()))
                            returnString += (MyBot.randomDoubleRange(0, 100) > 50) ? "," : ".";
                        //add a comma or period to it since it failed, if the last character isn't already a punctuation
                        sentence.Add(selectRandomWord());
                        returnString += " " + sentence.Last();
                    }
                }
            }
            returnString = formatSentence(returnString);
            Console.WriteLine("NEW SENTENCE: " + returnString);
            return returnString;
        }

        #region sentence formatting

        public string formatSentence(string sentence)
        {
            //remove white space at front
            while (sentence.First().Equals(' '))
                sentence = sentence.Remove(0, 1);

            //recapitalize
            string sentenceEnders = ".?!";
            for(int i = 0; i < sentence.Count(); i++)
            {
                if(Char.IsLower(sentence[i]) && i > 2)
                {
                    if (sentence[i - 1].Equals(' ') && sentenceEnders.Contains(sentence[i - 2]))
                    {
                        sentence = sentence.Insert(i, sentence[i].ToString().ToUpper());
                        sentence = sentence.Remove(i+1, 1);
                    }
                }
            }


            return sentence;
        }

        #endregion sentence formatting


        public Word selectRandomWord()
        {
            Word returnWord = null;

            if (vocabulary.Count() == 0)
                return returnWord;

            int currentBestIndex = -1;
            float currentSmallestDifference = 99;

            while (returnWord == null)
            {
                //pick the word with probability greater and closest to prob
                float prob = randomProbabilityForSentence();
                for (int j = 0; j < vocabulary.Count(); j++)
                {
                    float thisDifference = vocabulary.ElementAt(j).Value.wordProb - prob;
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
                    returnWord = vocabulary.ElementAt(currentBestIndex).Value;
                    break;
                }
            }
            return returnWord;
        }


        public static float randomProbabilityForSentence()
        {
            float prob = (float)MyBot.randomDoubleRange(minVocabWordProb * 0.2, maxVocabWordProb * 1.2);
            return prob;
        }


        #endregion Sentence Generation


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
        private float minWordAfterProb = 99, maxWordAfterProb = int.MinValue;

        #region Word stuff

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
            {
                wordAfterProbList.Add((float)count / totalWordAfterCount);
                float newProb = wordAfterProbList.Last();
                maxWordAfterProb = newProb > maxWordAfterProb ? newProb : maxWordAfterProb;
                minWordAfterProb = newProb < minWordAfterProb ? newProb : minWordAfterProb;
            }
        }

        #endregion Word stuff


        //THE IMPORTANT HEURISTIC METHOD
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

    }

    #endregion Word Class

}
