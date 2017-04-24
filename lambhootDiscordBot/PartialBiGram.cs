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
            var x = vocabulary;
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

        #region TextFile and Sentence Parsing

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
                //add the wordAfters if possible

                for(int j = i+1; j < stringWords.Count(); j++)
                {
                    vocabulary[stringWords[i]].addWordAfter(stringWords[j], j-(i+1));//j-original j) gives the correct gram number
                }    
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
                sentence.Add(selectVocabularyWord());//start random generation
                returnString += " " + sentence.Last();
            }
            else//handle starting sentence from input
            {
                string[] inputArray = input.Split(' ');
                string inputWord = inputArray.Last();
                if (!vocabulary.ContainsKey(inputWord))
                    sentence.Add(selectVocabularyWord());//if not in vocab, just start random sentence
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
                        Word nextWord = null;
                        while (nextWord == null)
                        {
                            nextWord = selectVocabularyWord();
                        }
                        sentence.Add(selectVocabularyWord());
                        returnString += " " + sentence.Last();
                    }
                }
            }
            returnString = formatSentence(returnString);
            Console.WriteLine("NEW SENTENCE: " + returnString);
            return returnString;
        }


        public string generateNewBiGramSentence(string input = null)
        {
            List<Word> sentence = new List<Word>();
            string returnString = "";
            int sentenceLength = (int)MyBot.randomDoubleRange(minSentenceLength, Math.Min(maxSentenceLength * 0.6, 25));

            //input handle
            if (input == null)
            {
                //choose first word
                sentence.Add(selectRandomWord());
                returnString += " " + sentence.Last();
            }
            else
            {
                //TODO: make it choose a good word from the input if possible
                string[] inputArray = input.Split(' ');
                string inputWord = inputArray.Last();

                    for (int i = 0; i < inputArray.Count(); i++)
                    {
                        if (vocabulary.ContainsKey(inputArray[i]))
                        {
                            sentence.Add(vocabulary[inputArray[i]]);
                        }
                        else
                            sentence.Add(new Word(inputArray[i]));
                        returnString += " " + sentence.Last();
                    }
            }

            //loop to build sentence
            while (sentence.Count() < sentenceLength)
            {
                int currentBestIndex = 0;
                float currentBestProb = float.MinValue;
                for (int i = 0; i < vocabulary.Count(); i++)
                {
                    float newProb = vocabulary.ElementAt(i).Value.probabilityGivenSentence(sentence);
                    if (newProb > currentBestProb)
                    {
                        currentBestProb = newProb;
                        currentBestIndex = i;
                    }
                    
                }
                //now add the word found
                if (currentBestIndex > 0)//it found a word other than the first default one
                {
                    sentence.Add(vocabulary.ElementAt(currentBestIndex).Value);
                    returnString += " " + sentence.Last();
                }
                else
                {
                    //it failed to find a proper word. add punctuation if necessary and choose new random word
                    if (!Char.IsPunctuation(returnString.Last()))
                        returnString += (MyBot.randomDoubleRange(0, 100) > 50) ? "," : ".";
                    //add a comma or period to it since it failed, if the last character isn't already a punctuation
                    Word nextWord = null;
                    while (nextWord == null)
                    {
                        nextWord = selectRandomWord();
                    }
                    sentence.Add(nextWord);
                    returnString += " " + sentence.Last();
                }
            }
            //sentence cleanup
            returnString = formatSentence(returnString);
            Console.WriteLine("NEW BAYES SENTENCE: " + returnString);
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
                    if ((sentence[i - 1].Equals(' ') && sentenceEnders.Contains(sentence[i - 2])) || i == 0)
                    {
                        sentence = sentence.Insert(i, sentence[i].ToString().ToUpper());
                        sentence = sentence.Remove(i+1, 1);
                    }
                }
            }


            return sentence;
        }

        #endregion sentence formatting


        public Word selectVocabularyWord()
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

        public Word selectRandomWord()
        {
            int index = (int)MyBot.randomDoubleRange(0, vocabulary.Count());
            return vocabulary.ElementAt(index).Value;
        }

        public static float randomProbabilityForSentence()
        {
            float prob = (float)MyBot.randomDoubleRange(minVocabWordProb * 0.2, maxVocabWordProb * 1.2);
            return prob;
        }


        #endregion Sentence Generation
    }


}
