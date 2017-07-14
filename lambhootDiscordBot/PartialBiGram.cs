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
        public static int maxBestWordChoices = 6;

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
            var x = vocabulary;
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
            //get actual vocabulary count
            int actualVocabCount = 0;
            foreach(Word w in vocabulary.Values)
            {
                actualVocabCount += w.getWordCount();
            }
            foreach (Word w in vocabulary.Values)
            {
                w.wordProb = (float)w.getWordCount() / actualVocabCount;
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
            List<List<Word>> sentences = new List<List<Word>>();
            sentences.Add(new List<Word>());//first sentence
            string returnString = "";

            //input handle
            if (input == null)
            {
                //choose first word
                sentences.Last().Add(selectRandomWord());
                returnString += " " + sentences.Last().Last();
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
                        sentences.Last().Add(vocabulary[inputArray[i]]);
                    }
                    else
                        sentences.Last().Add(new Word(inputArray[i]));
                    returnString += " " + sentences.Last().Last();
                }
            }

            int allSentenceLength = (int)MyBot.randomDoubleRange(minSentenceLength, Math.Min(maxSentenceLength * 0.6, 25));
            while (CountWordsOfSentences(sentences) < allSentenceLength) {
                int sentenceLength = (int)MyBot.randomDoubleRange(minSentenceLength, allSentenceLength);
                //loop to build sentence
                while (sentences.Last().Count() <= sentenceLength)
                {
                    if (returnString.Last().Equals('.') || returnString.Last().Equals('!') || returnString.Last().Equals('?'))
                    {
                        if (sentences.Last().Count() == 1 && returnString.Last().Equals('.'))//if sentence is a single word
                            sentences.RemoveAt(sentences.Count() - 1);
                        sentences.Add(new List<Word>());//add a new sentence
                        sentences.Last().Add(selectRandomWord());//start the new sentence
                        returnString += " " + sentences.Last().Last();
                        break;
                    }
                    int bestIndex = 0;
                    List<IndexProbPair> currentBestIndices = new List<IndexProbPair>();
                    int loopAttempts = 7;
                    while (loopAttempts > 0)
                    {
                        for (int i = 0; i < vocabulary.Count(); i++)
                        {
                            float newProb = vocabulary.ElementAt(i).Value.probabilityGivenSentence(sentences.Last());
                            tryAddBestIndex(currentBestIndices, i, newProb);
                        }
                        bestIndex = selectRandomIndex(currentBestIndices);
                        if (bestIndex == 0)
                            loopAttempts--;
                        else
                            break;
                    }
                    //now add the word found
                    if (bestIndex != 0)//it found a word other than the first default one
                    {
                        sentences.Last().Add(vocabulary.ElementAt(bestIndex).Value);
                        returnString += " " + sentences.Last().Last();
                    }
                    else
                    {
                        //it failed to find a proper word. add punctuation if necessary and choose new random word
                        if (!Char.IsPunctuation(returnString.Last()))
                            returnString += (MyBot.randomDoubleRange(0, 100) > 50) ? "," : ".";
                        //add a comma or period to it since it failed, if the last character isn't already a punctuation
                        sentences.Add(new List<Word>());//add a new sentence
                        sentences.Last().Add(selectRandomWord());//start the new sentence
                        returnString += " " + sentences.Last().Last();
                        break;
                    }
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
                if((Char.IsLower(sentence[i]) && i > 2) && !(sentence.Substring(i, 4).Equals("http")))//its lowercase and not a url
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
            int index = (int)MyBot.randomDoubleRange(1, vocabulary.Count());//skips first testing word
            return vocabulary.ElementAt(index).Value;
        }

        public static float randomProbabilityForSentence()
        {
            float prob = (float)MyBot.randomDoubleRange(minVocabWordProb * 0.2, maxVocabWordProb * 1.2);
            return prob;
        }


        #endregion Sentence Generation


        #region UTILS
        public static int CountWordsOfSentences(List<List<Word>> sentences)
        {
            int countSum = 0;
            foreach (List<Word> l in sentences)
                countSum += l.Count();
            return countSum;
        }

        public void tryAddBestIndex(List<IndexProbPair> currentBestIndices, int index, float prob)
        {
            if(currentBestIndices.Count() < maxBestWordChoices)
            {
                currentBestIndices.Add(new IndexProbPair(index, prob));
            }
            else {
                if(currentBestIndices.First().prob < prob)
                {
                    currentBestIndices.Remove(currentBestIndices.First());
                    currentBestIndices.Add(new IndexProbPair(index, prob));
                }
            }
            currentBestIndices.Sort();
        }

        public class IndexProbPair : IComparable<IndexProbPair>
        {
            public int index;
            public float prob;

            public IndexProbPair(int index, float prob)
            {
                this.index = index;
                this.prob = prob;
            }

            public int CompareTo(IndexProbPair other)
            {
                //for sorting, increasing order
                return prob.CompareTo(other.prob);
            }

        }

        public int selectRandomIndex(List<IndexProbPair> list)
        {
            int index = 0;
            float sumProbs = 0;
            foreach(IndexProbPair ipp in list){
                sumProbs += ipp.prob;
            }
            if (sumProbs == 0)
                return 0;

            while (index == 0) {
                index = (int)MyBot.randomDoubleRange(0, list.Count());
                if (list[index].prob == 0)//&& sumProbs != 0
                    index = 0;//try again
            }
            return list[index].index;
        }

        #endregion UTILS

    }

}
